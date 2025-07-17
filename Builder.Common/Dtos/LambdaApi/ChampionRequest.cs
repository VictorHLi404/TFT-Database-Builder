using Builder.Common.Enums;

namespace Builder.Common.Dtos.LambdaApi;

public class ChampionRequest
{
    public ChampionEnum ChampionName { get; set; }
    public int Level { get; set; }
    public List<ItemEnum> Items { get; set; } = [];
}