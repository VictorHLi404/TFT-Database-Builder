namespace Builder.Common.Dtos.LambdaApi;

public class TeamAlternativeStatisticsRequest
{
    public required TeamRequest Team { get; set; }
    public List<ChampionRequest> AlternativeChampions { get; set; } = [];
}