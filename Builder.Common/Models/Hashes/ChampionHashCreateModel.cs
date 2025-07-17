namespace Builder.Common.Models.Hashes;

public class ChampionHashCreateModel
{
    public required string ChampionName { get; set; }
    public required string Items { get; set; }
    public int Level { get; set; }
}