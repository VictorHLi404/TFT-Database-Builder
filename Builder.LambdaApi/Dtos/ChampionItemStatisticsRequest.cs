using Builder.Data.Enums;

namespace Builder.LambdaApi.Dtos;

public class ChampionItemStatisticsRequest
{
    public required Champion MainChampion { get; set; }
    public List<ItemEnum> items { get; set; } = [];
}