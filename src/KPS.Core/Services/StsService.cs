using System.Text;
using System.Text.Json;
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

      var response = await _httpClient.PostAsync(_options.StsEndpoint, content, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException($"STS service returned status {response.StatusCode}: {errorContent}");
      }

      var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

      // Extract and store STS artifacts in options
      ExtractAndStoreTokenArtifacts(responseContent);

      return _options.TokenXml;
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to authenticate with STS service: {ex.GetType().Name} - {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Creates a WS-Trust request envelope using factory pattern
  /// </summary>
  private string CreateWsTrustRequest(string username, string password)
  {
    var operation = XmlOperationFactory.CreateWsTrustRequestOperation(username, password, _options.StsEndpoint, _options.KpsEndpoint);
    return operation.Execute(new XmlDocument(), new XmlNamespaceManager(new NameTable()));
  }

  /// <summary>
  /// Extracts the STS artifacts from the response and stores them in options
  /// </summary>
  private void ExtractAndStoreTokenArtifacts(string response)
  {
    var operation = XmlOperationFactory.CreateExtractSamlTokenOperation(response);
    var jsonResult = operation.Execute(new XmlDocument(), new XmlNamespaceManager(new NameTable()));
    
    var artifacts = JsonSerializer.Deserialize<StsArtifacts>(jsonResult);
    if (artifacts == null)
    {
      throw new InvalidOperationException("Failed to deserialize STS artifacts");
    }

    // Store the artifacts in the options for use in subsequent SOAP requests
    _options.SigningKey = artifacts.BinarySecret;
    _options.AssertionId = artifacts.KeyIdentifier;
    _options.TokenXml = artifacts.TokenXml;
  }

  private class StsArtifacts
  {
    public string BinarySecret { get; set; } = string.Empty;
    public string KeyIdentifier { get; set; } = string.Empty;
    public string TokenXml { get; set; } = string.Empty;
  }
}
