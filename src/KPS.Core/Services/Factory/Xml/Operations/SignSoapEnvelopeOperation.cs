using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for signing SOAP envelope using HMAC-SHA1 (matching Go implementation)
/// </summary>
internal class SignSoapEnvelopeOperation : IXmlOperation
{
    private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    private const string DsigNamespace = "http://www.w3.org/2000/09/xmldsig#";

    private readonly KpsOptions _options;

    public SignSoapEnvelopeOperation(KpsOptions options)
    {
        _options = options;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        // Validate required options
        if (string.IsNullOrEmpty(_options.SigningKey))
        {
            throw new InvalidOperationException("SigningKey (BinarySecret from STS) is required for signing");
        }
        if (string.IsNullOrEmpty(_options.AssertionId))
        {
            throw new InvalidOperationException("AssertionId (KeyIdentifier from STS) is required for signing");
        }

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(5);
        var created = FormatTimestamp(now);
        var expiresStr = FormatTimestamp(expires);

        // Setup namespaces for XPath - must be done on the document's NameTable
        var localNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        localNsManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        localNsManager.AddNamespace("wsse", WsseNamespace);
        localNsManager.AddNamespace("wsu", WsuNamespace);

        // Get Security node - try multiple XPath expressions
        var securityNode = xmlDoc.SelectSingleNode("//wsse:Security", localNsManager)
            ?? xmlDoc.SelectSingleNode("//*[local-name()='Security']", localNsManager);

        if (securityNode == null)
        {
            throw new InvalidOperationException("Security header not found in SOAP envelope");
        }

        // Save existing children (TokenXML) to re-add in correct order
        var existingChildren = new List<XmlNode>();
        foreach (XmlNode child in securityNode.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element) // Skip whitespace nodes
            {
                existingChildren.Add(child);
            }
        }

        // Clear Security node children only (preserve attributes like s:mustUnderstand="1")
        while (securityNode.HasChildNodes)
        {
            securityNode.RemoveChild(securityNode.FirstChild!);
        }

        // Build timestamp XML string exactly like Go implementation
        var timestampXml = $@"<wsu:Timestamp xmlns:wsu=""{WsuNamespace}"" wsu:Id=""_0""><wsu:Created>{created}</wsu:Created><wsu:Expires>{expiresStr}</wsu:Expires></wsu:Timestamp>";

        // Canonicalize timestamp and compute digest (matching Go implementation)
        var c14nTimestamp = CanonicalizationExclusive(timestampXml);
        var timestampDigest = SHA1.HashData(c14nTimestamp);
        var digestValue = Convert.ToBase64String(timestampDigest);

        // Build SignedInfo XML string exactly like Go implementation
        var signedInfoXml = $@"<dsig:SignedInfo xmlns:dsig=""{DsigNamespace}"">
            <dsig:CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
            <dsig:SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#hmac-sha1""/>
            <dsig:Reference URI=""#_0"">
                <dsig:Transforms>
                    <dsig:Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#""/>
                </dsig:Transforms>
                <dsig:DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
                <dsig:DigestValue>{digestValue}</dsig:DigestValue>
            </dsig:Reference>
        </dsig:SignedInfo>";

        // Canonicalize SignedInfo and compute HMAC-SHA1 signature
        var c14nSignedInfo = CanonicalizationExclusive(signedInfoXml);
        var key = Convert.FromBase64String(_options.SigningKey.Trim());
        using var hmac = new HMACSHA1(key);
        var signatureValue = Convert.ToBase64String(hmac.ComputeHash(c14nSignedInfo));

        // Build complete Signature block exactly like Go implementation
        var signatureXml = $@"<dsig:Signature xmlns:dsig=""{DsigNamespace}"">
            {signedInfoXml}
            <dsig:SignatureValue>{signatureValue}</dsig:SignatureValue>
            <dsig:KeyInfo>
                <wsse:SecurityTokenReference xmlns:wsse=""{WsseNamespace}"">
                    <wsse:KeyIdentifier ValueType=""http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID"">{XmlEscape(_options.AssertionId)}</wsse:KeyIdentifier>
                </wsse:SecurityTokenReference>
            </dsig:KeyInfo>
        </dsig:Signature>";

        // Parse and import nodes into the document
        var timestampFragment = xmlDoc.CreateDocumentFragment();
        timestampFragment.InnerXml = timestampXml;
        securityNode.AppendChild(timestampFragment);

        // Re-add TokenXML (existing children)
        foreach (var child in existingChildren)
        {
            securityNode.AppendChild(child);
        }

        // Add Signature
        var signatureFragment = xmlDoc.CreateDocumentFragment();
        signatureFragment.InnerXml = signatureXml;
        securityNode.AppendChild(signatureFragment);

        return xmlDoc.OuterXml;
    }

    /// <summary>
    /// Format timestamp exactly like Go's tsISO function: 2006-01-02T15:04:05Z
    /// </summary>
    private static string FormatTimestamp(DateTime dt)
    {
        // Use 'Z' with quotes for literal Z character (not timezone offset)
        return dt.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// XML escape text (matching Go's xmlEscape function)
    /// </summary>
    private static string XmlEscape(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    /// <summary>
    /// Perform Exclusive XML Canonicalization (C14N) matching Go's c14nExclusive
    /// </summary>
    private static byte[] CanonicalizationExclusive(string xmlFragment)
    {
        var doc = new XmlDocument { PreserveWhitespace = false };
        doc.LoadXml(xmlFragment.Trim());

        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(doc);

        using var stream = (MemoryStream)transform.GetOutput(typeof(Stream));
        return stream.ToArray();
    }
}
