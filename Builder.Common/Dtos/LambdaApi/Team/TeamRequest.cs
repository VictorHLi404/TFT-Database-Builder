using Builder.Common.Dtos.LambdaApi.Champion;

namespace Builder.Common.Dtos.LambdaApi.Team;

public class TeamRequest
{
    public int Level { get; set; }
    public List<ChampionRequest> Champions { get; set; } = [];
}