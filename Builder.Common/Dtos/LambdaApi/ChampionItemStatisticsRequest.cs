using Builder.Common.Enums;

namespace Builder.Common.Dtos.LambdaApi;

public class ChampionItemStatisticsRequest
{
    public required Champion MainChampion { get; set; }
    public List<ItemEnum> items { get; set; } = [];
}