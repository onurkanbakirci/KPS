using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using KPS.Core.Models;

namespace KPS.Core.Services;

/// <summary>
/// Implementation of Security Token Service for KPS authentication
/// </summary>
public class StsService(HttpClient httpClient, ILogger<StsService> logger, KpsOptions options) : IStsService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<StsService> _logger = logger;
    private readonly KpsOptions _options = options;

    /// <summary>
    /// Gets a security token from the STS service using WS-Trust
    /// </summary>
    public async Task<string> GetSecurityTokenAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Requesting security token from STS service");

            var soapEnvelope = CreateWsTrustRequest(username, password);
            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            
            // Add required SOAP headers
            content.Headers.Add("SOAPAction", "\"http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue\"");

            var response = await _httpClient.PostAsync(_options.StsEndpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Received STS response");

            return ExtractTokenFromResponse(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security token from STS service");
            throw new InvalidOperationException("Failed to authenticate with STS service", ex);
        }
    }

    /// <summary>
    /// Creates a WS-Trust request envelope
    /// </summary>
    private static string CreateWsTrustRequest(string username, string password)
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
               xmlns:wsa=""http://www.w3.org/2005/08/addressing""
               xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd""
               xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""
               xmlns:wst=""http://docs.oasis-open.org/ws-sx/ws-trust/200512"">
  <soap:Header>
    <wsa:Action>http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue</wsa:Action>
    <wsa:To>{Uri.EscapeDataString("https://tckimlik.nvi.gov.tr/Service/STS")}</wsa:To>
    <wsa:MessageID>urn:uuid:{Guid.NewGuid()}</wsa:MessageID>
    <wsse:Security>
      <wsse:UsernameToken wsu:Id=""UsernameToken-1"">
        <wsse:Username>{XmlEscape(username)}</wsse:Username>
        <wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">{XmlEscape(password)}</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </soap:Header>
  <soap:Body>
    <wst:RequestSecurityToken>
      <wst:RequestType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue</wst:RequestType>
      <wst:TokenType>http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0</wst:TokenType>
      <wst:KeyType>http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer</wst:KeyType>
    </wst:RequestSecurityToken>
  </soap:Body>
</soap:Envelope>";

        return soapEnvelope;
    }

    /// <summary>
    /// Extracts the SAML token from the STS response
    /// </summary>
    private static string ExtractTokenFromResponse(string response)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);

            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("wst", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
            namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");

            var tokenNode = xmlDoc.SelectSingleNode("//saml:Assertion", namespaceManager);
            if (tokenNode == null)
            {
                throw new InvalidOperationException("SAML token not found in STS response");
            }

            return tokenNode.OuterXml;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse STS response", ex);
        }
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
