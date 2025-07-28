using System.Diagnostics.Contracts;
using Builder.Common.Dtos.LambdaApi.Champion;

namespace Builder.Common.Dtos.LambdaApi.Team;

public class TeamResponse
{
    public decimal AveragePlacement { get; set; }
    public List<ChampionResponse> Champions { get; set; } = [];
}