using Builder.Common.Enums;

namespace Builder.Common.Dtos.LambdaApi.Champion;

public class ChampionResponse
{
    public ChampionEnum ChampionName { get; set; }
    public decimal AveragePlacement { get; set; }
    public int Level { get; set; }
    public List<ItemEnum> Items { get; set; } = [];
}