using Builder.Common.Dtos.LambdaApi.Champion;
using Builder.Common.Dtos.LambdaApi.Team;
using Builder.LambdaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Builder.LambdaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TeamController : ControllerBase
{
    private readonly ILogger<TeamController> _logger;
    private readonly TeamService teamService;

    public TeamController(ILogger<TeamController> logger,
    TeamService teamService)
    {
        _logger = logger;
        this.teamService = teamService;
    }

    [HttpGet("TeamWinrate", Name = "GetTeamAveragePlacement")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamAveragePlacement([FromBody] TeamRequest request)
    {
        var results = await teamService.GetTeamCompAveragePlacement(request);

        var response = new TeamResponse
        {
            AveragePlacement = results.placement,
            Champions = results.champions.Select(x => new ChampionResponse
            {
                AveragePlacement = x.AveragePlacement,
                ChampionName = x.Champion,
                Level = x.ChampionLevel,
                Items = []
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet("TeamAlternativeComps", Name = "GetTeamAlternativeCompWinrates")]
    [ProducesResponseType(typeof(List<TeamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamAlternativeCompWinrates([FromBody] TeamAlternativeStatisticsRequest request)
    {
        var results = await teamService.GetTeamCompAlternatives(request.Team, request.AlternativeChampions);

        var response = results.Select(x => new TeamResponse
        {
            AveragePlacement = x.placement,
            Champions = x.champions.Select(t => new ChampionResponse
            {
                AveragePlacement = t.AveragePlacement,
                ChampionName = t.Champion,
                Level = t.ChampionLevel,
                Items = []
            }).ToList()
        }).ToList();

        return Ok(response);
    }

}
