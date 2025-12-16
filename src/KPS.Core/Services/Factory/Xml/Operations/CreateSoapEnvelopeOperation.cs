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
        var messageId = $"urn:uuid:{Guid.NewGuid()}";
        var actionUri = "http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis/Sorgula";
        var toUri = "https://kpsv2.nvi.gov.tr/Services/RoutingService.svc";

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<s:Envelope xmlns:s=""http://www.w3.org/2003/05/soap-envelope"">
  <s:Header>
    <a:MessageID xmlns:a=""http://www.w3.org/2005/08/addressing"">{messageId}</a:MessageID>
    <a:To xmlns:a=""http://www.w3.org/2005/08/addressing"" s:mustUnderstand=""1"">{toUri}</a:To>
    <a:Action xmlns:a=""http://www.w3.org/2005/08/addressing"" s:mustUnderstand=""1"">{actionUri}</a:Action>
    <wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" s:mustUnderstand=""1"">
      {_samlToken}
    </wsse:Security>
  </s:Header>
  <s:Body>
    <Sorgula xmlns=""http://kps.nvi.gov.tr/2025/08/01"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
      <kriterListesi>
        <TumKutukDogrulamaSorguKriteri>
          <Ad>{XmlEscape(_request.FirstName)}</Ad>
          <DogumAy>{XmlEscape(ZeroIfEmpty(_request.BirthMonth))}</DogumAy>
          <DogumGun>{XmlEscape(ZeroIfEmpty(_request.BirthDay))}</DogumGun>
          <DogumYil>{XmlEscape(_request.BirthYear)}</DogumYil>
          <KimlikNo>{XmlEscape(_request.TCNo)}</KimlikNo>
          <Soyad>{XmlEscape(_request.LastName)}</Soyad>
        </TumKutukDogrulamaSorguKriteri>
      </kriterListesi>
    </Sorgula>
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

    private static string ZeroIfEmpty(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "0" : text;
    }
}
