using System.Text;
using System.Xml;
using KPS.Core.Models;
using KPS.Core.Services.Abstract;
using KPS.Core.Services.Factory.Xml.Factories;

namespace KPS.Core.Services;

/// <summary>
/// Implementation of Security Token Service for KPS authentication
/// </summary>
public class StsService(HttpClient httpClient, KpsOptions options) : IStsService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly KpsOptions _options = options;

  /// <summary>
  /// Gets a security token from the STS service using WS-Trust
  /// </summary>
  public async Task<string> GetSecurityTokenAsync(string username, string password, CancellationToken cancellationToken = default)
  {
    try
    {
      var soapEnvelope = CreateWsTrustRequest(username, password);
      var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");

      // Add required SOAP headers
      content.Headers.Add("SOAPAction", "\"http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue\"");

      var response = await _httpClient.PostAsync(_options.StsEndpoint, content, cancellationToken);

      var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

      return ExtractTokenFromResponse(responseContent);
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException("Failed to authenticate with STS service", ex);
    }
  }

  /// <summary>
  /// Creates a WS-Trust request envelope using factory pattern
  /// </summary>
  private static string CreateWsTrustRequest(string username, string password)
  {
    var operation = XmlOperationFactory.CreateWsTrustRequestOperation(username, password);
    return operation.Execute(new XmlDocument(), new XmlNamespaceManager(new NameTable()));
  }

  /// <summary>
  /// Extracts the SAML token from the STS response using factory pattern
  /// </summary>
  private static string ExtractTokenFromResponse(string response)
  {
    var operation = XmlOperationFactory.CreateExtractSamlTokenOperation(response);
    return operation.Execute(new XmlDocument(), new XmlNamespaceManager(new NameTable()));
  }
}
