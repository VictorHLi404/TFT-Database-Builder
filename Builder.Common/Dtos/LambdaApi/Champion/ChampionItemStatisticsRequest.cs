using Builder.Common.Enums;

namespace Builder.Common.Dtos.LambdaApi.Champion;

public class ChampionItemStatisticsRequest
{
    public required ChampionRequest MainChampion { get; set; }
    public List<List<ItemEnum>> PossibleItemSets { get; set; } = [];
}