using Builder.Common.Dtos.LambdaApi;
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

}
