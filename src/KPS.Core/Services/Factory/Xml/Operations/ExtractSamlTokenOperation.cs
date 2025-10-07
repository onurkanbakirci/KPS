using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;
using KPS.Core.Services.Factory.Xml.Factories;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for extracting SAML token from STS response
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

            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("wst", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
            nsManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:1.0:assertion");

            var tokenNode = xmlDoc.SelectSingleNode("//saml:Assertion", nsManager);
            if (tokenNode == null)
            {
                throw new InvalidOperationException("SAML token not found in STS response");
            }

            return tokenNode.OuterXml;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse STS response", ex);
        }
    }
}
