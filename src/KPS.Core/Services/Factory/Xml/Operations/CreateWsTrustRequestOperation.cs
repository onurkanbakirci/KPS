using System.Text;
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
  private const string PasswordTextType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

  private readonly string _username;
  private readonly string _password;
  private readonly string _stsEndpoint;
  private readonly string _kpsEndpoint;

  public CreateWsTrustRequestOperation(string username, string password, string stsEndpoint, string kpsEndpoint)
  {
    _username = username;
    _password = password;
    _stsEndpoint = stsEndpoint;
    _kpsEndpoint = kpsEndpoint;
  }

  public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
  {
    var now = DateTime.UtcNow;
    var created = now.ToString("o"); // ISO 8601 format with milliseconds
    var expires = now.AddMinutes(5).ToString("o");
    var messageId = $"urn:uuid:{Guid.NewGuid()}";
    var usernameTokenId = $"uuid-{Guid.NewGuid()}";

    var sb = new StringBuilder();
    sb.Append("<?xml version='1.0' encoding='utf-8'?>");
    sb.Append($"<s:Envelope xmlns:s='{SoapNamespace}' xmlns:a='{WsaNamespace}' xmlns:u='{WsuNamespace}'>");
    sb.Append("<s:Header>");
    sb.Append($"<a:Action s:mustUnderstand='1'>{WsTrustIssueAction}</a:Action>");
    sb.Append($"<a:MessageID>{messageId}</a:MessageID>");
    sb.Append("<a:ReplyTo>");
    sb.Append("<a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>");
    sb.Append("</a:ReplyTo>");
    sb.Append($"<Sorgulayan>{XmlEscape(_username)}</Sorgulayan>");
    sb.Append($"<a:To s:mustUnderstand='1'>{_stsEndpoint}</a:To>");
    sb.Append($"<o:Security s:mustUnderstand='1' xmlns:o='{WsseNamespace}'>");
    sb.Append("<u:Timestamp u:Id='_0'>");
    sb.Append($"<u:Created>{created}</u:Created>");
    sb.Append($"<u:Expires>{expires}</u:Expires>");
    sb.Append("</u:Timestamp>");
    sb.Append($"<o:UsernameToken u:Id='{usernameTokenId}'>");
    sb.Append($"<o:Username>{XmlEscape(_username)}</o:Username>");
    sb.Append($"<o:Password Type='{PasswordTextType}'>{XmlEscape(_password)}</o:Password>");
    sb.Append("</o:UsernameToken>");
    sb.Append("</o:Security>");
    sb.Append("</s:Header>");
    sb.Append("<s:Body>");
    sb.Append($"<trust:RequestSecurityToken xmlns:trust='{WsTrustNamespace}'>");
    sb.Append($"<wsp:AppliesTo xmlns:wsp='{WspNamespace}'>");
    sb.Append("<a:EndpointReference>");
    sb.Append($"<a:Address>{_kpsEndpoint}</a:Address>");
    sb.Append("</a:EndpointReference>");
    sb.Append("</wsp:AppliesTo>");
    sb.Append($"<trust:RequestType>{WsTrustIssueRequestType}</trust:RequestType>");
    sb.Append("</trust:RequestSecurityToken>");
    sb.Append("</s:Body>");
    sb.Append("</s:Envelope>");

    return sb.ToString();
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
