using KPS.Core.Services.Factory.PersonSearch.Abstract;
using KPS.Core.Services.Factory.PersonSearch.Strategies;

namespace KPS.Core.Services.Factory.PersonSearch.Factories;

/// <summary>
/// Factory for creating person search strategies
/// </summary>
internal static class PersonSearchStrategyFactory
{
    /// <summary>
    /// Creates the appropriate search strategy based on the preferred component type
    /// </summary>
    /// <param name="preferredComponent">The preferred component type from the service response</param>
    /// <returns>The appropriate search strategy</returns>
    public static IPersonSearchStrategy CreateStrategy(string preferredComponent)
    {
        return preferredComponent switch
        {
            var pref when pref.Contains("yabanci") => new ForeignerSearchStrategy(),
            var pref when pref.Contains("tckisi") => new TurkishCitizenSearchStrategy(),
            var pref when pref.Contains("mavikart") => new BlueCardSearchStrategy(),
            _ => new TurkishCitizenSearchStrategy() // Default strategy
        };
    }
}
