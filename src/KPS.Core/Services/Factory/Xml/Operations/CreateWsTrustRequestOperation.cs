using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for creating WS-Trust request envelope
/// </summary>
internal class CreateWsTrustRequestOperation : IXmlOperation
{
    private readonly string _username;
    private readonly string _password;

    public CreateWsTrustRequestOperation(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
               xmlns:wsa=""http://www.w3.org/2005/08/addressing""
               xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
               xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""
               xmlns:wst=""http://docs.oasis-open.org/ws-sx/ws-trust/200512"">
  <soap:Header>
    <wsa:Action>http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</wsa:Action>
    <wsa:To>{Uri.EscapeDataString("https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13")}</wsa:To>
    <wsa:MessageID>urn:uuid:{Guid.NewGuid()}</wsa:MessageID>
    <wsse:Security>
      <wsse:UsernameToken wsu:Id=""UsernameToken-1"">
        <wsse:Username>{XmlEscape(_username)}</wsse:Username>
        <wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{XmlEscape(_password)}</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </soap:Header>
  <soap:Body>
    <wst:RequestSecurityToken>
      <wst:RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</wst:RequestType>
      <wst:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1</wst:TokenType>
      <wst:KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer</wst:KeyType>
    </wst:RequestSecurityToken>
  </soap:Body>
</soap:Envelope>";
    }

    private static string XmlEscape(string text)
    {
        return text.Replace("&", "&amp;")
                  .Replace("<", "&lt;")
                  .Replace(">", "&gt;")
                  .Replace("\"", "&quot;")
                  .Replace("'", "&apos;");
    }
}
