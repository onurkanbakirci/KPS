using KPS.Core.Services.Factory.PersonSearch.Abstract;

namespace KPS.Core.Services.Factory.PersonSearch.Strategies;

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
