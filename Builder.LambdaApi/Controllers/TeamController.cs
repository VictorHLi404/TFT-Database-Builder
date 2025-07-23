using Builder.Common.Dtos.LambdaApi.Team;
using Builder.Common.Dtos.LambdaApi.Champion;
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
    public async Task<IActionResult> GetTeamAveragePlacement([FromBody] TeamRequest request)
    {
        return Ok(await teamService.GetTeamCompAveragePlacement(request));
    }

    [HttpGet("TeamAlternativeComps", Name = "GetTeamAlternativeCompWinrates")]
    public async Task<IActionResult> GetTeamAlternativeCompWinrates([FromBody] TeamAlternativeStatisticsRequest request)
    {
        return Ok(await teamService.GetTeamCompAlternatives(request.Team, request.AlternativeChampions));
    }

}
