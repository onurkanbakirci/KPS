using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using KPS.Core.Models;

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
    public async Task<QueryResult> SendSoapRequestAsync(QueryRequest request, string samlToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Sending SOAP request to KPS service");

            var soapEnvelope = CreateSoapEnvelope(request, samlToken);
            var signedEnvelope = SignSoapEnvelope(soapEnvelope);
            
            var content = new StringContent(signedEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "\"http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula\"");

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
    private static string CreateSoapEnvelope(QueryRequest request, string samlToken)
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
               xmlns:wsa=""http://www.w3.org/2005/08/addressing""
               xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
               xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""
               xmlns:tns=""http://tckimlik.nvi.gov.tr/WS"">
  <soap:Header>
    <wsa:Action>http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula</wsa:Action>
    <wsa:To>https://tckimlik.nvi.gov.tr/Service/KPSPublic</wsa:To>
    <wsa:MessageID>urn:uuid:{Guid.NewGuid()}</wsa:MessageID>
    <wsse:Security>
      {samlToken}
    </wsse:Security>
  </soap:Header>
  <soap:Body>
    <tns:TCKimlikNoDogrula>
      <tns:TCKimlikNo>{XmlEscape(request.TCNo)}</tns:TCKimlikNo>
      <tns:Ad>{XmlEscape(request.FirstName)}</tns:Ad>
      <tns:Soyad>{XmlEscape(request.LastName)}</tns:Soyad>
      <tns:DogumYili>{XmlEscape(request.BirthYear)}</tns:DogumYili>
      <tns:DogumAy>{XmlEscape(request.BirthMonth)}</tns:DogumAy>
      <tns:DogumGun>{XmlEscape(request.BirthDay)}</tns:DogumGun>
    </tns:TCKimlikNoDogrula>
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
            namespaceManager.AddNamespace("tns", "http://tckimlik.nvi.gov.tr/WS");

            var resultNode = xmlDoc.SelectSingleNode("//tns:TCKimlikNoDogrulaResponse", namespaceManager);
            if (resultNode == null)
            {
                throw new InvalidOperationException("Invalid response format from KPS service");
            }

            var result = new QueryResult
            {
                Status = true,
                Code = ResultCodes.Success,
                Aciklama = "Query completed successfully"
            };

            // Check if the response indicates success or failure
            var resultValue = resultNode.InnerText?.Trim();
            if (string.IsNullOrEmpty(resultValue) || resultValue == "false")
            {
                result.Status = false;
                result.Code = ResultCodes.ErrorOrNotFound;
                result.Aciklama = "Person not found or data mismatch";
            }

            // Include raw response if configured
            if (_options.IncludeRawResponse)
            {
                result.Raw = response;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse SOAP response");
            throw new InvalidOperationException("Failed to parse KPS response", ex);
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
}
