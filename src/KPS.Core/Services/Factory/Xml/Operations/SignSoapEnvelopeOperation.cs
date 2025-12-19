using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for signing SOAP envelope using HMAC-SHA1 (matching Go implementation exactly)
/// </summary>
internal class SignSoapEnvelopeOperation : IXmlOperation
{
    private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    private const string DsigNamespace = "http://www.w3.org/2000/09/xmldsig#";
    private const string ExcC14NAlgorithm = "http://www.w3.org/2001/10/xml-exc-c14n#";
    private const string HmacSha1Algorithm = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
    private const string Sha1Algorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
    private const string SamlAssertionIdValueType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID";

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

        // Setup namespaces for XPath
        var localNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        localNsManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        localNsManager.AddNamespace("wsse", WsseNamespace);
        localNsManager.AddNamespace("wsu", WsuNamespace);

        // Get Security node
        var securityNode = xmlDoc.SelectSingleNode("//wsse:Security", localNsManager)
            ?? xmlDoc.SelectSingleNode("//*[local-name()='Security']", localNsManager);

        if (securityNode == null)
        {
            throw new InvalidOperationException("Security header not found in SOAP envelope");
        }

        // Save existing children (TokenXML) 
        var tokenXml = "";
        foreach (XmlNode child in securityNode.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                tokenXml = child.OuterXml;
                break;
            }
        }

        // Clear Security node children
        while (securityNode.HasChildNodes)
        {
            securityNode.RemoveChild(securityNode.FirstChild!);
        }

        // (1) Create Timestamp as string (exactly like Go implementation)
        var timestampXml = $"<wsu:Timestamp xmlns:wsu=\"{WsuNamespace}\" wsu:Id=\"_0\"><wsu:Created>{created}</wsu:Created><wsu:Expires>{expiresStr}</wsu:Expires></wsu:Timestamp>";

        // (2) Canonicalize & digest Timestamp
        var timestampC14N = C14NExclusive(timestampXml);
        var timestampDigest = SHA1.HashData(timestampC14N);
        var digestValue = Convert.ToBase64String(timestampDigest);

        // (3) Create SignedInfo as string (matching Go indentation)
        var signedInfoXml = $@"<dsig:SignedInfo xmlns:dsig=""{DsigNamespace}"">
            <dsig:CanonicalizationMethod Algorithm=""{ExcC14NAlgorithm}""/>
            <dsig:SignatureMethod Algorithm=""{HmacSha1Algorithm}""/>
            <dsig:Reference URI=""#_0"">
                <dsig:Transforms>
                    <dsig:Transform Algorithm=""{ExcC14NAlgorithm}""/>
                </dsig:Transforms>
                <dsig:DigestMethod Algorithm=""{Sha1Algorithm}""/>
                <dsig:DigestValue>{digestValue}</dsig:DigestValue>
            </dsig:Reference>
        </dsig:SignedInfo>";

        // (4) HMAC-SHA1(SignedInfo)
        var signedInfoC14N = C14NExclusive(signedInfoXml);
        var key = Convert.FromBase64String(_options.SigningKey.Trim());
        using var hmac = new HMACSHA1(key);
        var signatureValue = hmac.ComputeHash(signedInfoC14N);
        var signatureValueB64 = Convert.ToBase64String(signatureValue);

        // (5) Create Signature block (matching Go format exactly)
        // Note: SignedInfo is embedded WITHOUT xmlns:dsig since it's declared on Signature
        var signedInfoInner = $@"
            <dsig:CanonicalizationMethod Algorithm=""{ExcC14NAlgorithm}""/>
            <dsig:SignatureMethod Algorithm=""{HmacSha1Algorithm}""/>
            <dsig:Reference URI=""#_0"">
                <dsig:Transforms>
                    <dsig:Transform Algorithm=""{ExcC14NAlgorithm}""/>
                </dsig:Transforms>
                <dsig:DigestMethod Algorithm=""{Sha1Algorithm}""/>
                <dsig:DigestValue>{digestValue}</dsig:DigestValue>
            </dsig:Reference>
        ";

        var signatureXml = $@"<dsig:Signature xmlns:dsig=""{DsigNamespace}"">
            <dsig:SignedInfo>{signedInfoInner}</dsig:SignedInfo>
            <dsig:SignatureValue>{signatureValueB64}</dsig:SignatureValue>
            <dsig:KeyInfo>
                <wsse:SecurityTokenReference xmlns:wsse=""{WsseNamespace}"">
                    <wsse:KeyIdentifier ValueType=""{SamlAssertionIdValueType}"">{XmlEscape(_options.AssertionId)}</wsse:KeyIdentifier>
                </wsse:SecurityTokenReference>
            </dsig:KeyInfo>
        </dsig:Signature>";

        // (6) Add components to Security in correct order: Timestamp -> TokenXML -> Signature
        // Use XmlDocument fragment to properly add elements (avoiding string concatenation issues)
        var fragment = xmlDoc.CreateDocumentFragment();
        fragment.InnerXml = timestampXml + tokenXml + signatureXml;
        
        securityNode.AppendChild(fragment);

        return xmlDoc.OuterXml;
    }

    /// <summary>
    /// Performs Exclusive C14N canonicalization (matching Go's c14nExclusive function)
    /// </summary>
    private static byte[] C14NExclusive(string xmlFragment)
    {
        var trimmed = xmlFragment.Trim();
        var doc = new XmlDocument { PreserveWhitespace = false };
        doc.LoadXml(trimmed);

        var transform = new XmlDsigExcC14NTransform();
        transform.LoadInput(doc);

        using var stream = (MemoryStream)transform.GetOutput(typeof(Stream));
        return stream.ToArray();
    }

    /// <summary>
    /// Format timestamp exactly like Go's tsISO function: 2006-01-02T15:04:05Z
    /// </summary>
    private static string FormatTimestamp(DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// XML escape special characters
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
}
