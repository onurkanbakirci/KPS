using System.Xml;
using KPS.Core.Models;
using KPS.Core.Models.Result;
using KPS.Core.Services.Factory.PersonSearch.Factories;
using KPS.Core.Services.Factory.Xml.Abstract;

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

            // Check for SOAP fault (namespace-agnostic)
            var faultNode = xmlDoc.SelectSingleNode("//*[local-name()='Fault']", nsManager);
            if (faultNode != null)
            {
                var faultCode = GetNodeText(faultNode, ".//*[local-name()='faultcode']");
                if (string.IsNullOrEmpty(faultCode))
                {
                    faultCode = GetNodeText(faultNode, ".//*[local-name()='Code']/*[local-name()='Value']");
                }
                var faultString = GetNodeText(faultNode, ".//*[local-name()='faultstring']");
                if (string.IsNullOrEmpty(faultString))
                {
                    faultString = GetNodeText(faultNode, ".//*[local-name()='Reason']/*[local-name()='Text']");
                }
                throw new InvalidOperationException($"SOAP fault received: {(string.IsNullOrEmpty(faultCode) ? "Unknown" : faultCode)} - {(string.IsNullOrEmpty(faultString) ? "Unknown error" : faultString)}");
            }

            // Look for the response node (namespace-agnostic using local-name())
            var resultNode = xmlDoc.SelectSingleNode("//*[local-name()='SorgulaResponse']", nsManager);
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

                // Map short names to PersonTypes constants
                var personType = name switch
                {
                    "tc" => PersonTypes.TurkishCitizen,
                    "mavi" => PersonTypes.BlueCard,
                    "yabanci" => PersonTypes.Foreigner,
                    _ => string.Empty
                };

                // Get identity number
                var kimlik = GetNodeText(kisiNode, ".//*[local-name()='TCKimlikNo']");
                if (string.IsNullOrEmpty(kimlik))
                {
                    kimlik = GetNodeText(kisiNode, ".//*[local-name()='KimlikNo']");
                }

                // Get name information
                var ad = GetNodeText(kisiNode, ".//*[local-name()='TemelBilgisi']/*[local-name()='Ad']");
                var soyad = GetNodeText(kisiNode, ".//*[local-name()='TemelBilgisi']/*[local-name()='Soyad']");
                var uyruk = GetNodeText(kisiNode, ".//*[local-name()='TemelBilgisi']/*[local-name()='Uyruk']");

                // Get birth date
                var dogumYil = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='DogumTarih']/*[local-name()='Yil']");
                var dogumAy = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='DogumTarih']/*[local-name()='Ay']");
                var dogumGun = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='DogumTarih']/*[local-name()='Gun']");
                var dogumTarih = JoinDate(dogumYil, dogumAy, dogumGun);

                // Get death date
                var olumYil = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='OlumTarih']/*[local-name()='Yil']");
                var olumAy = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='OlumTarih']/*[local-name()='Ay']");
                var olumGun = GetNodeText(kisiNode, ".//*[local-name()='DurumBilgisi']/*[local-name()='OlumTarih']/*[local-name()='Gun']");
                var olumTarih = JoinDate(olumYil, olumAy, olumGun);

                // Create PersonInfo object
                var personInfo = new PersonInfo
                {
                    Type = personType,
                    IdentityNumber = kimlik ?? string.Empty,
                    FirstName = ad ?? string.Empty,
                    LastName = soyad ?? string.Empty,
                    Nationality = string.IsNullOrEmpty(uyruk) ? null : uyruk,
                    BirthDate = string.IsNullOrEmpty(dogumTarih) ? null : dogumTarih,
                    DeathDate = string.IsNullOrEmpty(olumTarih) ? null : olumTarih
                };

                // Also populate Extra dictionary for backward compatibility
                var extra = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(kimlik))
                {
                    extra["KimlikNo"] = kimlik;
                }
                if (!string.IsNullOrEmpty(ad))
                {
                    extra["Ad"] = ad;
                }
                if (!string.IsNullOrEmpty(soyad))
                {
                    extra["Soyad"] = soyad;
                }
                if (!string.IsNullOrEmpty(uyruk))
                {
                    extra["Uyruk"] = uyruk;
                }
                if (!string.IsNullOrEmpty(dogumTarih))
                {
                    extra["DogumTarih"] = dogumTarih;
                }
                if (!string.IsNullOrEmpty(olumTarih))
                {
                    extra["OlumTarih"] = olumTarih;
                }

                var result = new QueryResult
                {
                    Status = code == (int)ResultCodes.Success,
                    Code = (ResultCodes)code,
                    Aciklama = aciklama ?? "Query completed",
                    Person = personInfo,
                    Extra = extra
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
                Code = ResultCodes.ErrorOrNotFound,
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

    private static string Pad2(string s)
    {
        s = s?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(s)) return string.Empty;
        if (s.Length == 1) return "0" + s;
        return s;
    }

    private static string JoinDate(string year, string month, string day)
    {
        year = year?.Trim() ?? string.Empty;
        month = month?.Trim() ?? string.Empty;
        day = day?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(year)) return string.Empty;
        if (string.IsNullOrEmpty(month) && string.IsNullOrEmpty(day)) return year;
        if (string.IsNullOrEmpty(day)) return $"{year}-{Pad2(month)}";
        return $"{year}-{Pad2(month)}-{Pad2(day)}";
    }
}