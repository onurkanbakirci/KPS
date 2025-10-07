using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for signing SOAP envelope
/// </summary>
internal class SignSoapEnvelopeOperation : IXmlOperation
{
    private readonly KpsOptions _options;

    public SignSoapEnvelopeOperation(KpsOptions options)
    {
        _options = options;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(5);

        // Add timestamp
        var securityNode = xmlDoc.SelectSingleNode("//wsse:Security", nsManager);
        if (securityNode == null)
        {
            throw new InvalidOperationException("Security header not found");
        }

        var timestampNode = CreateTimestampNode(xmlDoc, now, expires);
        securityNode.AppendChild(timestampNode);

        // Create SignedInfo
        var signedInfo = CreateSignedInfoNode(xmlDoc, timestampNode);

        // Create Signature element
        var signatureElement = CreateSignatureElement(xmlDoc, signedInfo);
        securityNode.AppendChild(signatureElement);

        return xmlDoc.OuterXml;
    }

    private XmlElement CreateTimestampNode(XmlDocument xmlDoc, DateTime now, DateTime expires)
    {
        var timestampNode = xmlDoc.CreateElement("wsu", "Timestamp", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        timestampNode.SetAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", "_0");

        var createdNode = xmlDoc.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        createdNode.InnerText = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        timestampNode.AppendChild(createdNode);

        var expiresNode = xmlDoc.CreateElement("wsu", "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        expiresNode.InnerText = expires.ToString("yyyy-MM-ddTHH:mm:ssZ");
        timestampNode.AppendChild(expiresNode);

        return timestampNode;
    }

    private XmlElement CreateSignedInfoNode(XmlDocument xmlDoc, XmlElement timestampNode)
    {
        var signedInfo = xmlDoc.CreateElement("dsig", "SignedInfo", "http://www.w3.org/2000/09/xmldsig#");

        var c14nMethod = xmlDoc.CreateElement("dsig", "CanonicalizationMethod", "http://www.w3.org/2000/09/xmldsig#");
        c14nMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
        signedInfo.AppendChild(c14nMethod);

        var signatureMethod = xmlDoc.CreateElement("dsig", "SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
        signatureMethod.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#hmac-sha1");
        signedInfo.AppendChild(signatureMethod);

        var reference = CreateReferenceNode(xmlDoc, timestampNode);
        signedInfo.AppendChild(reference);

        return signedInfo;
    }

    private XmlElement CreateReferenceNode(XmlDocument xmlDoc, XmlElement timestampNode)
    {
        var reference = xmlDoc.CreateElement("dsig", "Reference", "http://www.w3.org/2000/09/xmldsig#");
        reference.SetAttribute("URI", "#_0");

        var transforms = xmlDoc.CreateElement("dsig", "Transforms", "http://www.w3.org/2000/09/xmldsig#");
        var transform = xmlDoc.CreateElement("dsig", "Transform", "http://www.w3.org/2000/09/xmldsig#");
        transform.SetAttribute("Algorithm", "http://www.w3.org/2001/10/xml-exc-c14n#");
        transforms.AppendChild(transform);
        reference.AppendChild(transforms);

        var digestMethod = xmlDoc.CreateElement("dsig", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
        digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2000/09/xmldsig#sha1");
        reference.AppendChild(digestMethod);

        // Calculate digest
        var c14nTransform = new XmlDsigExcC14NTransform();
        c14nTransform.LoadInput(timestampNode);
        var c14nTimestamp = (byte[])c14nTransform.GetOutput(typeof(byte[]));
        var timestampDigest = SHA1.HashData(c14nTimestamp);

        var digestValue = xmlDoc.CreateElement("dsig", "DigestValue", "http://www.w3.org/2000/09/xmldsig#");
        digestValue.InnerText = Convert.ToBase64String(timestampDigest);
        reference.AppendChild(digestValue);

        return reference;
    }

    private XmlElement CreateSignatureElement(XmlDocument xmlDoc, XmlElement signedInfo)
    {
        var signatureElement = xmlDoc.CreateElement("dsig", "Signature", "http://www.w3.org/2000/09/xmldsig#");
        signatureElement.AppendChild(signedInfo);

        // Calculate signature
        var c14nTransformSI = new XmlDsigExcC14NTransform();
        c14nTransformSI.LoadInput(signedInfo);
        var c14nSignedInfo = (byte[])c14nTransformSI.GetOutput(typeof(byte[]));

        using var hmac = new HMACSHA1(Convert.FromBase64String(_options.SigningKey));
        var signatureValue = hmac.ComputeHash(c14nSignedInfo);

        var signatureValueElement = xmlDoc.CreateElement("dsig", "SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
        signatureValueElement.InnerText = Convert.ToBase64String(signatureValue);
        signatureElement.AppendChild(signatureValueElement);

        var keyInfo = CreateKeyInfoElement(xmlDoc);
        signatureElement.AppendChild(keyInfo);

        return signatureElement;
    }

    private XmlElement CreateKeyInfoElement(XmlDocument xmlDoc)
    {
        var keyInfo = xmlDoc.CreateElement("dsig", "KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
        var securityTokenReference = xmlDoc.CreateElement("wsse", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        var keyIdentifier = xmlDoc.CreateElement("wsse", "KeyIdentifier", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        keyIdentifier.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID");
        keyIdentifier.InnerText = _options.AssertionId;
        securityTokenReference.AppendChild(keyIdentifier);
        keyInfo.AppendChild(securityTokenReference);

        return keyInfo;
    }
}
