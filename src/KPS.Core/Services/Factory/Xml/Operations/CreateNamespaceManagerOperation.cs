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
        // Use SOAP 1.2 namespace
        nsManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
        nsManager.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
        nsManager.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
        nsManager.AddNamespace("wsa", "http://www.w3.org/2005/08/addressing");
        nsManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        nsManager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");

        // Return empty string as this operation only modifies the namespace manager
        return string.Empty;
    }
}
