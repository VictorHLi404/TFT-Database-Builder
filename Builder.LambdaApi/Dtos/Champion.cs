using Builder.Data.Enums;

namespace Builder.LambdaApi.Dtos;

public class Champion
{
    public ChampionEnum ChampionName { get; set; }
    public int Level { get; set; }
    public List<ItemEnum> Items { get; set; } = [];
}