using System.Globalization;
using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for creating WS-Trust request envelope
/// </summary>
internal class CreateWsTrustRequestOperation : IXmlOperation
{
  // Namespace constants
  private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
  private const string WsaNamespace = "http://www.w3.org/2005/08/addressing";
  private const string WsTrustNamespace = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
  private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
  private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
  private const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";

  // WS-Trust action and type constants
  private const string WsTrustIssueAction = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue";
  private const string WsTrustIssueRequestType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue";
  private const string WsTrustSymmetricKeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/SymmetricKey";
  private const string SamlTokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";
  private const string PasswordTextType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

  // Endpoint constants
  private const string StsEndpoint = "https://kimlikdogrulama.nvi.gov.tr/Services/Issuer.svc/IWSTrust13";
  private const string KpsEndpoint = "https://kpsv2.nvi.gov.tr/Services/RoutingService.svc";

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

    return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<s:Envelope xmlns:s=""{SoapNamespace}"" xmlns:a=""{WsaNamespace}"" xmlns:wst=""{WsTrustNamespace}"" xmlns:wsse=""{WsseNamespace}"" xmlns:wsu=""{WsuNamespace}"" xmlns:wsp=""{WspNamespace}"">
  <s:Header>
    <a:MessageID>{messageId}</a:MessageID>
    <a:To>{StsEndpoint}</a:To>
    <a:Action>{WsTrustIssueAction}</a:Action>
    <wsse:Security s:mustUnderstand=""1"">
      <wsu:Timestamp wsu:Id=""_0"">
        <wsu:Created>{created}</wsu:Created>
        <wsu:Expires>{expires}</wsu:Expires>
      </wsu:Timestamp>
      <wsse:UsernameToken wsu:Id=""Me"">
        <wsse:Username>{XmlEscape(_username)}</wsse:Username>
        <wsse:Password Type=""{PasswordTextType}"">{XmlEscape(_password)}</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </s:Header>
  <s:Body>
    <wst:RequestSecurityToken>
      <wst:TokenType>{SamlTokenType}</wst:TokenType>
      <wst:RequestType>{WsTrustIssueRequestType}</wst:RequestType>
      <wsp:AppliesTo>
        <a:EndpointReference>
          <a:Address>{KpsEndpoint}</a:Address>
        </a:EndpointReference>
      </wsp:AppliesTo>
      <wst:KeyType>{WsTrustSymmetricKeyType}</wst:KeyType>
    </wst:RequestSecurityToken>
  </s:Body>
</s:Envelope>";
  }

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
