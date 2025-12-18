using System.Text;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Models.Request;
using KPS.Core.Models.Result;
using KPS.Core.Services.Abstract;
using KPS.Core.Services.Factory.Xml.Factories;
using System.Text.Json;

namespace KPS.Core.Services;

/// <summary>
/// Implementation of SOAP service for KPS queries with HMAC-SHA1 signing
/// </summary>
public class SoapService(HttpClient httpClient, KpsOptions options) : ISoapService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly KpsOptions _options = options;

    /// <summary>
    /// Sends a signed SOAP request to the KPS service
    /// </summary>
    public async Task<QueryResult> SendSoapRequestAsync(CitizenVerificationRequest request, string samlToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use TokenXml from options (set by StsService) instead of the samlToken parameter
            var tokenXml = string.IsNullOrEmpty(_options.TokenXml) ? samlToken : _options.TokenXml;
            
            var soapEnvelope = CreateSoapEnvelope(request, tokenXml);
            var signedEnvelope = SignSoapEnvelope(soapEnvelope);
            
            var content = new StringContent(signedEnvelope, Encoding.UTF8, "application/soap+xml");

            var response = await _httpClient.PostAsync(_options.KpsEndpoint, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"KPS service returned status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return ParseSoapResponse(responseContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to query KPS service: {ex.GetType().Name} - {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates the SOAP envelope for the KPS query
    /// </summary>
    private string CreateSoapEnvelope(CitizenVerificationRequest request, string samlToken)
    {
        var operation = XmlOperationFactory.CreateSoapEnvelopeOperation(request, samlToken, _options);
        return operation.Execute(new XmlDocument(), new XmlNamespaceManager(new NameTable()));
    }

    /// <summary>
    /// Signs the SOAP envelope using HMAC-SHA1 with proper XML canonicalization
    /// </summary>
    private string SignSoapEnvelope(string soapEnvelope)
    {
        try
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(soapEnvelope);

            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            XmlOperationFactory.CreateNamespaceManagerOperation().Execute(xmlDoc, nsManager);
            
            var operation = XmlOperationFactory.CreateSignSoapEnvelopeOperation(_options);
            return operation.Execute(xmlDoc, nsManager);
        }
        catch (Exception ex)
        {
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
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            
            XmlOperationFactory.CreateNamespaceManagerOperation().Execute(xmlDoc, nsManager);
            var operation = XmlOperationFactory.CreateParseSoapResponseOperation(response, _options);
            
            var resultJson = operation.Execute(xmlDoc, nsManager);
            return JsonSerializer.Deserialize<QueryResult>(resultJson)
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse KPS response", ex);
        }
    }
}
