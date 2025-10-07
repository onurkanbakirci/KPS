namespace KPS.Core.Services.Factory;

/// <summary>
/// Strategy interface for determining person search order
/// </summary>
internal interface IPersonSearchStrategy
{
    List<(string path, string name)> GetSearchOrder();
}

/// <summary>
/// Strategy for Turkish citizen priority search
/// </summary>
internal class TurkishCitizenSearchStrategy : IPersonSearchStrategy
{
    public List<(string path, string name)> GetSearchOrder()
    {
        return new List<(string path, string name)>
        {
            ("//*[local-name()='TCVatandasiKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "tc"),
            ("//*[local-name()='MaviKartliKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "mavi"),
            ("//*[local-name()='YabanciKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "yabanci")
        };
    }
}

/// <summary>
/// Strategy for foreigner priority search
/// </summary>
internal class ForeignerSearchStrategy : IPersonSearchStrategy
{
    public List<(string path, string name)> GetSearchOrder()
    {
        return new List<(string path, string name)>
        {
            ("//*[local-name()='YabanciKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "yabanci"),
            ("//*[local-name()='TCVatandasiKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "tc"),
            ("//*[local-name()='MaviKartliKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "mavi")
        };
    }
}

/// <summary>
/// Strategy for blue card holder priority search
/// </summary>
internal class BlueCardSearchStrategy : IPersonSearchStrategy
{
    public List<(string path, string name)> GetSearchOrder()
    {
        return new List<(string path, string name)>
        {
            ("//*[local-name()='MaviKartliKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "mavi"),
            ("//*[local-name()='TCVatandasiKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "tc"),
            ("//*[local-name()='YabanciKisiKutukleri' and not(@*[local-name()='nil']='true')]/*[local-name()='KisiBilgisi']", "yabanci")
        };
    }
}

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
