namespace KPS.Core.Services.Factory.PersonSearch.Abstract;

/// <summary>
/// Strategy interface for determining person search order
/// </summary>
internal interface IPersonSearchStrategy
{
    List<(string path, string name)> GetSearchOrder();
}
