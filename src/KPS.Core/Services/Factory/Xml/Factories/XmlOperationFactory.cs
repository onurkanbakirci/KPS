using KPS.Core.Models;
using KPS.Core.Models.Request;
using KPS.Core.Services.Factory.Xml.Abstract;
using KPS.Core.Services.Factory.Xml.Operations;

namespace KPS.Core.Services.Factory.Xml.Factories;

/// <summary>
/// Factory for creating XML operations
/// </summary>
internal static class XmlOperationFactory
{
    public static IXmlOperation CreateSoapEnvelopeOperation(CitizenVerificationRequest request, string samlToken, KpsOptions options)
    {
        return new CreateSoapEnvelopeOperation(request, samlToken, options);
    }

    public static IXmlOperation CreateSignSoapEnvelopeOperation(KpsOptions options)
    {
        return new SignSoapEnvelopeOperation(options);
    }

    public static IXmlOperation CreateWsTrustRequestOperation(string username, string password, string stsEndpoint, string kpsEndpoint)
    {
        return new CreateWsTrustRequestOperation(username, password, stsEndpoint, kpsEndpoint);
    }

    public static IXmlOperation CreateExtractSamlTokenOperation(string response)
    {
        return new ExtractSamlTokenOperation(response);
    }

    public static IXmlOperation CreateParseSoapResponseOperation(string response, KpsOptions options)
    {
        return new ParseSoapResponseOperation(response, options);
    }

    public static IXmlOperation CreateNamespaceManagerOperation()
    {
        return new CreateNamespaceManagerOperation();
    }
}
