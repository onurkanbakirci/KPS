using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using KPS.Core.Models;
using KPS.Core.Models.Request;
using KPS.Core.Models.Result;
using KPS.Core.Services.Abstract;
using KPS.Core.Services.Factory;

namespace KPS.Core.Services;

/// <summary>
/// Implementation of SOAP service for KPS queries with HMAC-SHA1 signing
/// </summary>
public class SoapService(HttpClient httpClient, ILogger<SoapService> logger, KpsOptions options) : ISoapService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<SoapService> _logger = logger;
    private readonly KpsOptions _options = options;

    /// <summary>
    /// Sends a signed SOAP request to the KPS service
    /// </summary>
    public async Task<QueryResult> SendSoapRequestAsync(CitizenVerificationRequest request, string samlToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Sending SOAP request to KPS service");

            var soapEnvelope = CreateSoapEnvelope(request, samlToken);
            var signedEnvelope = SignSoapEnvelope(soapEnvelope);
            
            var content = new StringContent(signedEnvelope, Encoding.UTF8, "application/soap+xml");
            content.Headers.Add("SOAPAction", "\"http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis/Sorgula\"");

            var response = await _httpClient.PostAsync(_options.KpsEndpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Received KPS response");

            return ParseSoapResponse(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SOAP request to KPS service");
            throw new InvalidOperationException("Failed to query KPS service", ex);
        }
    }

    /// <summary>
    /// Creates the SOAP envelope for the KPS query
    /// </summary>
    private static string CreateSoapEnvelope(CitizenVerificationRequest request, string samlToken)
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
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
      {samlToken}
    </wsse:Security>
  </soap:Header>
  <soap:Body>
    <tns:Sorgula xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
      <tns:kriterListesi>
        <tns:TumKutukDogrulamaSorguKriteri>
          <tns:Ad>{XmlEscape(request.FirstName)}</tns:Ad>
          <tns:DogumAy>{XmlEscape(ZeroIfEmpty(request.BirthMonth))}</tns:DogumAy>
          <tns:DogumGun>{XmlEscape(ZeroIfEmpty(request.BirthDay))}</tns:DogumGun>
          <tns:DogumYil>{XmlEscape(request.BirthYear)}</tns:DogumYil>
          <tns:KimlikNo>{XmlEscape(request.TCNo)}</tns:KimlikNo>
          <tns:Soyad>{XmlEscape(request.LastName)}</tns:Soyad>
        </tns:TumKutukDogrulamaSorguKriteri>
      </tns:kriterListesi>
    </tns:Sorgula>
  </soap:Body>
</soap:Envelope>";

        return soapEnvelope;
    }

    /// <summary>
    /// Signs the SOAP envelope using HMAC-SHA1
    /// </summary>
    private string SignSoapEnvelope(string soapEnvelope)
    {
        try
        {
            // Extract the body content for signing
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(soapEnvelope);

            var bodyNode = xmlDoc.SelectSingleNode("//soap:Body", CreateNamespaceManager(xmlDoc));
            if (bodyNode == null)
            {
                throw new InvalidOperationException("SOAP Body not found");
            }

            var bodyContent = bodyNode.InnerXml;
            var bodyBytes = Encoding.UTF8.GetBytes(bodyContent);

            // Create HMAC-SHA1 signature
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_options.Username + _options.Password));
            var signature = hmac.ComputeHash(bodyBytes);
            var signatureBase64 = Convert.ToBase64String(signature);

            // Add signature to the SOAP header
            var securityNode = xmlDoc.SelectSingleNode("//wsse:Security", CreateNamespaceManager(xmlDoc));
            if (securityNode != null)
            {
                var signatureElement = xmlDoc.CreateElement("wsse", "Signature", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
                signatureElement.InnerText = signatureBase64;
                securityNode.AppendChild(signatureElement);
            }

            return xmlDoc.OuterXml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign SOAP envelope");
            throw new InvalidOperationException("Failed to sign SOAP request", ex);
        }
    }

    /// <summary>
    /// Parses the SOAP response into a QueryResult
    /// </summary>
    private QueryResult ParseSoapResponse(string response)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);

            var namespaceManager = CreateNamespaceManager(xmlDoc);
            namespaceManager.AddNamespace("tns", "http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis");

            // Look for the response node
            var resultNode = xmlDoc.SelectSingleNode("//tns:SorgulaResponse", namespaceManager);
            if (resultNode == null)
            {
                throw new InvalidOperationException("Invalid response format from KPS service");
            }

            // Get the preferred component type from DoluBilesenler
            var doluBilesenler = GetNodeText(xmlDoc, "//*[local-name()='DoluBilesenler']/*[contains(local-name(), 'DogrulaServisDoluBilesen')]");
            var prefer = doluBilesenler?.ToLower().Replace(" ", "") ?? "";

            // Use strategy pattern to determine search order
            var searchStrategy = PersonSearchStrategyFactory.CreateStrategy(prefer);
            var searchOrder = searchStrategy.GetSearchOrder();

            // Search for person information in the preferred order
            foreach (var (path, name) in searchOrder)
            {
                var kisiNode = xmlDoc.SelectSingleNode(path);
                if (kisiNode == null) continue;

                // Get status code
                var kodStr = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='Durum']/*[local-name()='Kod']");
                if (string.IsNullOrEmpty(kodStr))
                {
                    kodStr = GetNodeText(kisiNode, ".//*[local-name()='Durum']/*[local-name()='Kod']");
                }

                if (string.IsNullOrEmpty(kodStr)) continue;

                if (!int.TryParse(kodStr, out var code)) continue;

                // Get description
                var aciklama = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='Durum']/*[local-name()='Aciklama']");
                if (string.IsNullOrEmpty(aciklama))
                {
                    aciklama = GetNodeText(kisiNode, ".//*[local-name()='Durum']/*[local-name()='Aciklama']");
                }

                var result = new QueryResult
                {
                    Status = code == 1,
                    Code = code,
                    Aciklama = aciklama ?? "Query completed"
                };

                // Include raw response if configured
                if (_options.IncludeRawResponse)
                {
                    result.Raw = response;
                }

                return result;
            }

            // No person information found
            var notFoundResult = new QueryResult
            {
                Status = false,
                Code = 2,
                Aciklama = "Kayıt bulunamadı"
            };

            if (_options.IncludeRawResponse)
            {
                notFoundResult.Raw = response;
            }

            return notFoundResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse SOAP response");
            throw new InvalidOperationException("Failed to parse KPS response", ex);
        }
    }

    /// <summary>
    /// Gets the text content of a node using XPath
    /// </summary>
    private static string GetNodeText(XmlNode node, string xpath)
    {
        try
        {
            var foundNode = node.SelectSingleNode(xpath);
            return foundNode?.InnerText?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Creates XML namespace manager for SOAP documents
    /// </summary>
    private static XmlNamespaceManager CreateNamespaceManager(XmlDocument xmlDoc)
    {
        var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
        namespaceManager.AddNamespace("wsa", "http://www.w3.org/2005/08/addressing");
        namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        namespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
        return namespaceManager;
    }

    /// <summary>
    /// Escapes XML special characters
    /// </summary>
    private static string XmlEscape(string text)
    {
        return text.Replace("&", "&amp;")
                  .Replace("<", "&lt;")
                  .Replace(">", "&gt;")
                  .Replace("\"", "&quot;")
                  .Replace("'", "&apos;");
    }

    /// <summary>
    /// Returns "0" if the string is empty or whitespace, otherwise returns the original string
    /// </summary>
    private static string ZeroIfEmpty(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "0" : text;
    }
}
