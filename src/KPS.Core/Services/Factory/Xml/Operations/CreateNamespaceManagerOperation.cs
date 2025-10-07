using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for creating XML namespace manager
/// </summary>
internal class CreateNamespaceManagerOperation : IXmlOperation
{
    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
        nsManager.AddNamespace("wsa", "http://www.w3.org/2005/08/addressing");
        nsManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        
        // Return empty string as this operation only modifies the namespace manager
        return string.Empty;
    }
}
