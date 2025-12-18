using System.Security.Cryptography;
using System.Text;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for signing SOAP envelope using HMAC-SHA1 (matching working KpsTest implementation)
/// </summary>
internal class SignSoapEnvelopeOperation : IXmlOperation
{
    private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
    private const string Wsse11Namespace = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";
    private const string DsigNamespace = "http://www.w3.org/2000/09/xmldsig#";
    private const string ExcC14NAlgorithm = "http://www.w3.org/2001/10/xml-exc-c14n#";
    private const string HmacSha1Algorithm = "http://www.w3.org/2000/09/xmldsig#hmac-sha1";
    private const string Sha1Algorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
    private const string SamlAssertionIdValueType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID";
    private const string SamlTokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";

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

        // Build timestamp string exactly like the working version
        var timestampString = BuildTimestampString(now, expires);

        // Calculate digest using simple string-based SHA1 (like working version)
        var digestValue = ComputeSignatureSHA1(timestampString);

        // Build SignedInfo string (with default namespace, not dsig: prefix)
        var signedInfoString = BuildSignedInfoString(digestValue);

        // Calculate signature using HMAC-SHA1
        var signatureValue = ComputeSignatureHMACSHA1(_options.SigningKey, signedInfoString);

        // Setup namespaces for XPath
        var localNsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        localNsManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        localNsManager.AddNamespace("o", WsseNamespace);

        // Get Security node
        var securityNode = xmlDoc.SelectSingleNode("//o:Security", localNsManager)
            ?? xmlDoc.SelectSingleNode("//*[local-name()='Security']", localNsManager);

        if (securityNode == null)
        {
            throw new InvalidOperationException("Security header not found in SOAP envelope");
        }

        // Get the existing token (EncryptedData) from inside Security
        var existingTokenXml = "";
        foreach (XmlNode child in securityNode.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Element)
            {
                existingTokenXml = child.OuterXml;
                break;
            }
        }

        // Clear Security node children
        while (securityNode.HasChildNodes)
        {
            securityNode.RemoveChild(securityNode.FirstChild!);
        }

        // Build the complete Security content: Timestamp -> EncryptedData -> Signature
        var securityContent = new StringBuilder();
        securityContent.Append(timestampString);
        securityContent.Append(existingTokenXml);
        securityContent.Append(BuildSignatureString(signedInfoString, signatureValue, _options.AssertionId));

        // Parse and append the content
        var tempDoc = new XmlDocument();
        tempDoc.LoadXml($"<root xmlns:o=\"{WsseNamespace}\" xmlns:u=\"{WsuNamespace}\">{securityContent}</root>");
        
        foreach (XmlNode child in tempDoc.DocumentElement!.ChildNodes)
        {
            var importedNode = xmlDoc.ImportNode(child, true);
            securityNode.AppendChild(importedNode);
        }

        return xmlDoc.OuterXml;
    }

    /// <summary>
    /// Builds timestamp string exactly like the working version
    /// </summary>
    private static string BuildTimestampString(DateTime now, DateTime expires)
    {
        var sb = new StringBuilder();
        sb.Append($"<u:Timestamp xmlns:u=\"{WsuNamespace}\" u:Id=\"_0\">");
        sb.Append($"<u:Created>{now.ToString("o")}</u:Created>");
        sb.Append($"<u:Expires>{expires.ToString("o")}</u:Expires>");
        sb.Append("</u:Timestamp>");
        return sb.ToString();
    }

    /// <summary>
    /// Builds SignedInfo string with default namespace (not prefixed)
    /// </summary>
    private static string BuildSignedInfoString(string digestValue)
    {
        var sb = new StringBuilder();
        sb.Append($"<SignedInfo xmlns=\"{DsigNamespace}\">");
        sb.Append($"<CanonicalizationMethod Algorithm=\"{ExcC14NAlgorithm}\"></CanonicalizationMethod>");
        sb.Append($"<SignatureMethod Algorithm=\"{HmacSha1Algorithm}\"></SignatureMethod>");
        sb.Append("<Reference URI=\"#_0\">");
        sb.Append("<Transforms>");
        sb.Append($"<Transform Algorithm=\"{ExcC14NAlgorithm}\"></Transform>");
        sb.Append("</Transforms>");
        sb.Append($"<DigestMethod Algorithm=\"{Sha1Algorithm}\"></DigestMethod>");
        sb.Append($"<DigestValue>{digestValue}</DigestValue>");
        sb.Append("</Reference>");
        sb.Append("</SignedInfo>");
        return sb.ToString();
    }

    /// <summary>
    /// Builds complete Signature element string
    /// </summary>
    private static string BuildSignatureString(string signedInfoString, string signatureValue, string assertionId)
    {
        var sb = new StringBuilder();
        sb.Append($"<Signature xmlns=\"{DsigNamespace}\">");
        sb.Append(signedInfoString);
        sb.Append($"<SignatureValue>{signatureValue}</SignatureValue>");
        sb.Append("<KeyInfo>");
        sb.Append($"<o:SecurityTokenReference k:TokenType=\"{SamlTokenType}\" xmlns:k=\"{Wsse11Namespace}\">");
        sb.Append($"<o:KeyIdentifier ValueType=\"{SamlAssertionIdValueType}\">{assertionId}</o:KeyIdentifier>");
        sb.Append("</o:SecurityTokenReference>");
        sb.Append("</KeyInfo>");
        sb.Append("</Signature>");
        return sb.ToString();
    }

    /// <summary>
    /// Computes SHA1 hash of input string (matching working version)
    /// </summary>
    private static string ComputeSignatureSHA1(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA1.HashData(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Computes HMAC-SHA1 signature (matching working version)
    /// </summary>
    private static string ComputeSignatureHMACSHA1(string key, string data)
    {
        var secret = Convert.FromBase64String(key);
        var body = Encoding.UTF8.GetBytes(data);
        using var hmacsha1 = new HMACSHA1(secret);
        var hashmessage = hmacsha1.ComputeHash(body);
        return Convert.ToBase64String(hashmessage);
    }
}
