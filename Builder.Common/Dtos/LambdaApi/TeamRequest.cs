namespace Builder.Common.Dtos.LambdaApi;

public class TeamRequest
{
    public int Level { get; set; }
    public List<ChampionRequest> Champions { get; set; } = [];
}