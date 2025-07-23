using Builder.Common.Enums;

namespace Builder.Common.Dtos.LambdaApi.Champion;

public class ChampionItemStatisticsRequest
{
    public required ChampionRequest MainChampion { get; set; }
    public List<ItemEnum> items { get; set; } = [];
}