using System.Text.Json;
using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for extracting STS artifacts (BinarySecret, KeyIdentifier, Token) from STS response
/// </summary>
internal class ExtractSamlTokenOperation : IXmlOperation
{
    private readonly string _response;

    public ExtractSamlTokenOperation(string response)
    {
        _response = response;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        try
        {
            xmlDoc.LoadXml(_response);

            // Use SOAP 1.2 namespace
            nsManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            nsManager.AddNamespace("wst", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
            nsManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

            // Extract BinarySecret (HMAC key)
            var binarySecretNode = xmlDoc.SelectSingleNode("//wst:BinarySecret", nsManager);
            if (binarySecretNode == null)
            {
                throw new InvalidOperationException("BinarySecret not found in STS response");
            }
            var binarySecret = binarySecretNode.InnerText.Trim();

            // Extract KeyIdentifier (SAML Assertion ID)
            var keyIdentifierNode = xmlDoc.SelectSingleNode("//wsse:KeyIdentifier", nsManager);
            if (keyIdentifierNode == null)
            {
                throw new InvalidOperationException("KeyIdentifier not found in STS response");
            }
            var keyIdentifier = keyIdentifierNode.InnerText.Trim();

            // Extract RequestedSecurityToken inner XML (EncryptedData)
            var requestedTokenNode = xmlDoc.SelectSingleNode("//wst:RequestedSecurityToken", nsManager);
            if (requestedTokenNode == null)
            {
                throw new InvalidOperationException("RequestedSecurityToken not found in STS response");
            }
            var tokenXml = requestedTokenNode.InnerXml.Trim();

            // Return as JSON for easy deserialization
            var result = new
            {
                BinarySecret = binarySecret,
                KeyIdentifier = keyIdentifier,
                TokenXml = tokenXml
            };

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse STS response", ex);
        }
    }
}
