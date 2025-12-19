using System.Text.Json;
using System.Xml;
using KPS.Core.Services.Factory.Xml.Abstract;

namespace KPS.Core.Services.Factory.Xml.Operations;

/// <summary>
/// Operation for extracting STS artifacts (BinarySecret, KeyIdentifier, Token) from STS response
/// Matches Go implementation exactly - uses string manipulation to preserve original XML
/// </summary>
internal class ExtractSamlTokenOperation : IXmlOperation
{
    private const string SoapNamespace = "http://www.w3.org/2003/05/soap-envelope";
    private const string WsTrustNamespace = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
    private const string WsseNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

    private readonly string _response;

    public ExtractSamlTokenOperation(string response)
    {
        _response = response;
    }

    public string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager)
    {
        try
        {
            // Use XmlDocument only for BinarySecret and KeyIdentifier (simple text nodes)
            xmlDoc.LoadXml(_response);

            // Setup namespace manager for XPath queries
            nsManager.AddNamespace("s", SoapNamespace);
            nsManager.AddNamespace("wst", WsTrustNamespace);
            nsManager.AddNamespace("wsse", WsseNamespace);

            // Extract BinarySecret (HMAC key)
            var binarySecretNode = xmlDoc.SelectSingleNode("//wst:BinarySecret", nsManager);
            if (binarySecretNode == null)
            {
                throw new InvalidOperationException("BinarySecret not found in STS response");
            }
            var binarySecret = binarySecretNode.InnerText.Trim();

            // Extract KeyIdentifier (SAML Assertion ID)
            var keyIdentifierNode = xmlDoc.SelectSingleNode("//wsse:KeyIdentifier", nsManager);
            if (keyIdentifierNode == null)
            {
                throw new InvalidOperationException("KeyIdentifier not found in STS response");
            }
            var keyIdentifier = keyIdentifierNode.InnerText.Trim();

            // CRITICAL: Extract RequestedSecurityToken inner XML using string manipulation
            // (matching Go's extractInnerXMLOfTagAnyNS function)
            // This preserves the EXACT original XML without any normalization
            var tokenXml = ExtractInnerXMLOfTagAnyNS(_response, "RequestedSecurityToken");

            if (string.IsNullOrWhiteSpace(binarySecret) || 
                string.IsNullOrWhiteSpace(keyIdentifier) || 
                string.IsNullOrWhiteSpace(tokenXml))
            {
                throw new InvalidOperationException("STS parse error (secret/keyID/token missing)");
            }

            // Return as JSON for easy deserialization
            var result = new
            {
                BinarySecret = binarySecret.Trim(),
                KeyIdentifier = keyIdentifier.Trim(),
                TokenXml = tokenXml.Trim()
            };

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse STS response", ex);
        }
    }

    /// <summary>
    /// Extract inner XML of a tag using string manipulation (matching Go's extractInnerXMLOfTagAnyNS)
    /// This preserves the exact original XML without any parsing/normalization
    /// </summary>
    private static string ExtractInnerXMLOfTagAnyNS(string xmlStr, string localName)
    {
        var low = xmlStr.ToLower();
        var needleOpen = "<" + localName.ToLower();
        var idx = low.IndexOf(needleOpen, StringComparison.Ordinal);
        
        if (idx < 0)
        {
            // Try with namespace prefix (e.g., "wst:RequestedSecurityToken")
            needleOpen = ":" + localName.ToLower();
            idx = low.IndexOf(needleOpen, StringComparison.Ordinal);
            if (idx < 0)
            {
                return "";
            }
            // Find the opening '<' before the prefix
            idx = low.LastIndexOf('<', idx);
            if (idx < 0)
            {
                return "";
            }
        }

        // Find the '>' that closes the opening tag
        var gt = low.IndexOf('>', idx);
        if (gt < 0)
        {
            return "";
        }
        var start = gt + 1;

        // Find the closing tag
        var closeTag = "</" + localName.ToLower() + ">";
        var end = low.IndexOf(closeTag, start, StringComparison.Ordinal);
        
        if (end < 0)
        {
            // Try to find closing tag with namespace prefix
            var cand = low.IndexOf(localName.ToLower() + ">", start, StringComparison.Ordinal);
            if (cand >= 0)
            {
                var pre = low.LastIndexOf("</", cand, StringComparison.Ordinal);
                if (pre >= 0 && pre >= start)
                {
                    end = pre;
                }
            }
            if (end < 0)
            {
                return "";
            }
        }

        // Return the substring from original string (not lowercased) to preserve exact formatting
        return xmlStr.Substring(start, end - start);
    }
}
