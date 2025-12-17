using System.Globalization;
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
    var now = DateTime.UtcNow;
    var created = now.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
    var expires = now.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
    var messageId = $"urn:uuid:{Guid.NewGuid()}";
    var stsEndpoint = "https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13";
    var kpsEndpoint = "https://kpsv2.nvi.gov.tr/Services/RoutingService.svc";

    return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"" xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:wst=""http://docs.oasis-open.org/ws-sx/ws-trust/200512"" xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:wsp=""http://schemas.xmlsoap.org/ws/2004/09/policy"">
  <s:Header>
    <a:MessageID>{messageId}</a:MessageID>
    <a:To>{stsEndpoint}</a:To>
    <a:Action>http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</a:Action>
    <wsse:Security s:mustUnderstand=""1"">
      <wsu:Timestamp wsu:Id=""_0"">
        <wsu:Created>{created}</wsu:Created>
        <wsu:Expires>{expires}</wsu:Expires>
      </wsu:Timestamp>
      <wsse:UsernameToken wsu:Id=""Me"">
        <wsse:Username>{XmlEscape(_username)}</wsse:Username>
        <wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{XmlEscape(_password)}</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </s:Header>
  <s:Body>
    <wst:RequestSecurityToken>
      <wst:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1</wst:TokenType>
      <wst:RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</wst:RequestType>
      <wsp:AppliesTo>
        <a:EndpointReference>
          <a:Address>{kpsEndpoint}</a:Address>
        </a:EndpointReference>
      </wsp:AppliesTo>
      <wst:KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey</wst:KeyType>
    </wst:RequestSecurityToken>
  </s:Body>
</s:Envelope>";
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
