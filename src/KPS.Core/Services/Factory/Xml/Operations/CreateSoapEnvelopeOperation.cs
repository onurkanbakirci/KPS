using System.Text;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Models.Request;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for creating SOAP envelope
/// </summary>
internal class CreateSoapEnvelopeOperation : IXmlOperation
{
  // Namespace constants
  private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
  private const string WsaNamespace = "http://www.w3.org/2005/08/addressing";
  private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
  private const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
  private const string SorgulayanNamespace = "http://kps.nvi.gov.tr/2011/01/01/sorgulayan";

  // KPS service constants - using 2023/07/01 like the working version
  private const string KpsBodyNamespace = "http://kps.nvi.gov.tr/2023/07/01";
  private const string KpsActionUri = "http://kps.nvi.gov.tr/2023/07/01/KimlikNoSorgulaAdresServis/Sorgula";

  private readonly CitizenVerificationRequest _request;
  private readonly string _samlToken;
  private readonly KpsOptions _options;

  public CreateSoapEnvelopeOperation(CitizenVerificationRequest request, string samlToken, KpsOptions options)
  {
    _request = request;
    _samlToken = samlToken;
    _options = options;
  }

  public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
  {
    var messageId = $"urn:uuid:{Guid.NewGuid()}";

    var sb = new StringBuilder();
    sb.Append($"<s:Envelope xmlns:s=\"{SoapNamespace}\" xmlns:a=\"{WsaNamespace}\" xmlns:u=\"{WsuNamespace}\" xmlns:ns=\"{KpsBodyNamespace}\">");
    sb.Append("<s:Header>");
    sb.Append($"<a:Action s:mustUnderstand=\"1\">{KpsActionUri}</a:Action>");
    sb.Append($"<Sorgulayan xmlns=\"{SorgulayanNamespace}\">{XmlEscape(_options.Username)}</Sorgulayan>");
    sb.Append($"<a:MessageID>{messageId}</a:MessageID>");
    sb.Append("<a:ReplyTo>");
    sb.Append("<a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>");
    sb.Append("</a:ReplyTo>");
    sb.Append($"<a:To s:mustUnderstand=\"1\">{_options.KpsEndpoint}</a:To>");
    sb.Append($"<o:Security s:mustUnderstand=\"1\" xmlns:o=\"{WsseNamespace}\">");
    // Token will be inserted here by SignSoapEnvelopeOperation
    sb.Append($"{_samlToken}");
    sb.Append("</o:Security>");
    sb.Append("</s:Header>");
    sb.Append("<s:Body>");
    sb.Append("<ns:Sorgula>");
    sb.Append("<ns:kriterListesi>");
    sb.Append("<ns:KimlikNoileAdresSorguKriteri>");
    sb.Append($"<ns:DogumAy>{XmlEscape(ZeroIfEmpty(_request.BirthMonth))}</ns:DogumAy>");
    sb.Append($"<ns:DogumGun>{XmlEscape(ZeroIfEmpty(_request.BirthDay))}</ns:DogumGun>");
    sb.Append($"<ns:DogumYil>{XmlEscape(_request.BirthYear)}</ns:DogumYil>");
    sb.Append($"<ns:KimlikNo>{XmlEscape(_request.TCNo)}</ns:KimlikNo>");
    sb.Append("</ns:KimlikNoileAdresSorguKriteri>");
    sb.Append("</ns:kriterListesi>");
    sb.Append("</ns:Sorgula>");
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

  private static string ZeroIfEmpty(string text)
  {
    return string.IsNullOrWhiteSpace(text) ? "0" : text;
  }
}
