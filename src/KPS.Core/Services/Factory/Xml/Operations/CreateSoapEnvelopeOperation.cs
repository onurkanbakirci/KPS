using System.Xml;
using KPS.Core.Models.Request;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for creating SOAP envelope
/// </summary>
internal class CreateSoapEnvelopeOperation : IXmlOperation
{
    private readonly CitizenVerificationRequest _request;
    private readonly string _samlToken;

    public CreateSoapEnvelopeOperation(CitizenVerificationRequest request, string samlToken)
    {
        _request = request;
        _samlToken = samlToken;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
               xmlns:wsa=""http://www.w3.org/2005/08/addressing""
               xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
               xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""
               xmlns:tns=""http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis"">
  <soap:Header>
    <wsa:Action>http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis/Sorgula</wsa:Action>
    <wsa:To>https://kpsv2.nvi.gov.tr/Services/RoutingService.svc</wsa:To>
    <wsa:MessageID>urn:uuid:{Guid.NewGuid()}</wsa:MessageID>
    <wsse:Security>
      {_samlToken}
    </wsse:Security>
  </soap:Header>
  <soap:Body>
    <tns:Sorgula xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
      <tns:kriterListesi>
        <tns:TumKutukDogrulamaSorguKriteri>
          <tns:Ad>{XmlEscape(_request.FirstName)}</tns:Ad>
          <tns:DogumAy>{XmlEscape(ZeroIfEmpty(_request.BirthMonth))}</tns:DogumAy>
          <tns:DogumGun>{XmlEscape(ZeroIfEmpty(_request.BirthDay))}</tns:DogumGun>
          <tns:DogumYil>{XmlEscape(_request.BirthYear)}</tns:DogumYil>
          <tns:KimlikNo>{XmlEscape(_request.TCNo)}</tns:KimlikNo>
          <tns:Soyad>{XmlEscape(_request.LastName)}</tns:Soyad>
        </tns:TumKutukDogrulamaSorguKriteri>
      </tns:kriterListesi>
    </tns:Sorgula>
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

    private static string ZeroIfEmpty(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "0" : text;
    }
}
