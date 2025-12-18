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

        // Create nodes using XmlDocument (original working approach)
        var timestampNode = CreateTimestampNode(xmlDoc, now, expires);
        var signedInfo = CreateSignedInfoNode(xmlDoc, timestampNode);
        var signatureElement = CreateSignatureElement(xmlDoc, signedInfo);

        // Add in correct order: Timestamp -> TokenXML -> Signature (matching Go implementation)
        securityNode.AppendChild(timestampNode);
        foreach (var child in existingChildren)
        {
            securityNode.AppendChild(child);
        }
        securityNode.AppendChild(signatureElement);

        return xmlDoc.OuterXml;
    }

    private XmlElement CreateTimestampNode(XmlDocument xmlDoc, DateTime now, DateTime expires)
    {
        var timestampNode = xmlDoc.CreateElement("wsu", "Timestamp", WsuNamespace);
        timestampNode.SetAttribute("Id", WsuNamespace, "_0");

        var createdNode = xmlDoc.CreateElement("wsu", "Created", WsuNamespace);
        createdNode.InnerText = FormatTimestamp(now);
        timestampNode.AppendChild(createdNode);

        var expiresNode = xmlDoc.CreateElement("wsu", "Expires", WsuNamespace);
        expiresNode.InnerText = FormatTimestamp(expires);
        timestampNode.AppendChild(expiresNode);

        return timestampNode;
    }

    private XmlElement CreateSignedInfoNode(XmlDocument xmlDoc, XmlElement timestampNode)
    {
        var signedInfo = xmlDoc.CreateElement("dsig", "SignedInfo", DsigNamespace);

        var c14nMethod = xmlDoc.CreateElement("dsig", "CanonicalizationMethod", DsigNamespace);
        c14nMethod.SetAttribute("Algorithm", ExcC14NAlgorithm);
        signedInfo.AppendChild(c14nMethod);

        var signatureMethod = xmlDoc.CreateElement("dsig", "SignatureMethod", DsigNamespace);
        signatureMethod.SetAttribute("Algorithm", HmacSha1Algorithm);
        signedInfo.AppendChild(signatureMethod);

        var reference = CreateReferenceNode(xmlDoc, timestampNode);
        signedInfo.AppendChild(reference);

        return signedInfo;
    }

    private XmlElement CreateReferenceNode(XmlDocument xmlDoc, XmlElement timestampNode)
    {
        var reference = xmlDoc.CreateElement("dsig", "Reference", DsigNamespace);
        reference.SetAttribute("URI", "#_0");

        var transforms = xmlDoc.CreateElement("dsig", "Transforms", DsigNamespace);
        var transform = xmlDoc.CreateElement("dsig", "Transform", DsigNamespace);
        transform.SetAttribute("Algorithm", ExcC14NAlgorithm);
        transforms.AppendChild(transform);
        reference.AppendChild(transforms);

        var digestMethod = xmlDoc.CreateElement("dsig", "DigestMethod", DsigNamespace);
        digestMethod.SetAttribute("Algorithm", Sha1Algorithm);
        reference.AppendChild(digestMethod);

        // Calculate digest - Create a temporary document with the timestamp node
        var tempDoc = new XmlDocument();
        tempDoc.LoadXml(timestampNode.OuterXml);

        var c14nTransform = new XmlDsigExcC14NTransform();
        c14nTransform.LoadInput(tempDoc);

        using var c14nStream = (MemoryStream)c14nTransform.GetOutput(typeof(Stream));
        var c14nTimestamp = c14nStream.ToArray();
        var timestampDigest = SHA1.HashData(c14nTimestamp);

        var digestValue = xmlDoc.CreateElement("dsig", "DigestValue", DsigNamespace);
        digestValue.InnerText = Convert.ToBase64String(timestampDigest);
        reference.AppendChild(digestValue);

        return reference;
    }

    private XmlElement CreateSignatureElement(XmlDocument xmlDoc, XmlElement signedInfo)
    {
        var signatureElement = xmlDoc.CreateElement("dsig", "Signature", DsigNamespace);
        signatureElement.AppendChild(signedInfo);

        // Calculate signature - Create a temporary document with the signed info node
        var tempDoc = new XmlDocument();
        tempDoc.LoadXml(signedInfo.OuterXml);

        var c14nTransformSI = new XmlDsigExcC14NTransform();
        c14nTransformSI.LoadInput(tempDoc);

        using var c14nStream = (MemoryStream)c14nTransformSI.GetOutput(typeof(Stream));
        var c14nSignedInfo = c14nStream.ToArray();

        using var hmac = new HMACSHA1(Convert.FromBase64String(_options.SigningKey.Trim()));
        var signatureValue = hmac.ComputeHash(c14nSignedInfo);

        var signatureValueElement = xmlDoc.CreateElement("dsig", "SignatureValue", DsigNamespace);
        signatureValueElement.InnerText = Convert.ToBase64String(signatureValue);
        signatureElement.AppendChild(signatureValueElement);

        var keyInfo = CreateKeyInfoElement(xmlDoc);
        signatureElement.AppendChild(keyInfo);

        return signatureElement;
    }

    private XmlElement CreateKeyInfoElement(XmlDocument xmlDoc)
    {
        var keyInfo = xmlDoc.CreateElement("dsig", "KeyInfo", DsigNamespace);
        var securityTokenReference = xmlDoc.CreateElement("wsse", "SecurityTokenReference", WsseNamespace);
        var keyIdentifier = xmlDoc.CreateElement("wsse", "KeyIdentifier", WsseNamespace);
        keyIdentifier.SetAttribute("ValueType", SamlAssertionIdValueType);
        keyIdentifier.InnerText = _options.AssertionId;
        securityTokenReference.AppendChild(keyIdentifier);
        keyInfo.AppendChild(securityTokenReference);

        return keyInfo;
    }

    /// <summary>
    /// Format timestamp exactly like Go's tsISO function: 2006-01-02T15:04:05Z
    /// </summary>
    private static string FormatTimestamp(DateTime dt)
    {
        // Use 'Z' with quotes for literal Z character (not timezone offset)
        return dt.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
