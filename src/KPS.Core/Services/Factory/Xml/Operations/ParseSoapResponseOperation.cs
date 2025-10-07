using System.Xml;
using KPS.Core.Models;
using KPS.Core.Models.Result;
using KPS.Core.Services.Factory.PersonSearch.Factories;
using KPS.Core.Services.Factory.Xml.Abstract;
using KPS.Core.Services.Factory.Xml.Factories;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for parsing SOAP response
/// </summary>
internal class ParseSoapResponseOperation : IXmlOperation
{
    private readonly string _response;
    private readonly KpsOptions _options;

    public ParseSoapResponseOperation(string response, KpsOptions options)
    {
        _response = response;
        _options = options;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        if (string.IsNullOrWhiteSpace(_response))
        {
            throw new ArgumentException("Response cannot be null or empty", nameof(_response));
        }

        try
        {
            // Set secure XML resolver settings
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                ValidationType = ValidationType.None,
                MaxCharactersFromEntities = 1024,
                MaxCharactersInDocument = 1024 * 1024 // 1MB limit
            };

            using (var stringReader = new StringReader(_response))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                xmlDoc.Load(xmlReader);
            }

            nsManager.AddNamespace("tns", "http://kps.nvi.gov.tr/2025/08/01/TumKutukDogrulaServis");

            // Check for SOAP fault
            var faultNode = xmlDoc.SelectSingleNode("//soap:Fault", nsManager);
            if (faultNode != null)
            {
                var faultCode = faultNode.SelectSingleNode("faultcode")?.InnerText ?? "Unknown";
                var faultString = faultNode.SelectSingleNode("faultstring")?.InnerText ?? "Unknown error";
                throw new InvalidOperationException($"SOAP fault received: {faultCode} - {faultString}");
            }

            // Look for the response node
            var resultNode = xmlDoc.SelectSingleNode("//tns:SorgulaResponse", nsManager);
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

                if (_options.IncludeRawResponse)
                {
                    result.Raw = _response;
                }

                return System.Text.Json.JsonSerializer.Serialize(result);
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
                notFoundResult.Raw = _response;
            }

            return System.Text.Json.JsonSerializer.Serialize(notFoundResult);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse KPS response", ex);
        }
    }

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
}
