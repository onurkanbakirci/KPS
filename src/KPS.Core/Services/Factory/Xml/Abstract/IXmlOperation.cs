using System.Xml;

namespace KPS.Core.Services.Factory.Xml.Abstract;

/// <summary>
/// Interface for XML operations
/// </summary>
internal interface IXmlOperation
{
    string Execute(XmlDocument xmlDoc, XmlNamespaceManager nsManager);
}
