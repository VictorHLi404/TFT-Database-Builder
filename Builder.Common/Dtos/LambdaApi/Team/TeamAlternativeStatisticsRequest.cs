using Builder.Common.Dtos.LambdaApi.Champion;

namespace Builder.Common.Dtos.LambdaApi.Team;

public class TeamAlternativeStatisticsRequest
{
    public required TeamRequest Team { get; set; }
    public List<ChampionRequest> AlternativeChampions { get; set; } = [];
}