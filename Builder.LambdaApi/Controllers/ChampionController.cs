using Builder.Common.Dtos.LambdaApi;
using Builder.LambdaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Builder.LambdaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ChampionController : ControllerBase
{
    private readonly ILogger<ChampionController> _logger;
    private readonly ChampionService championService;

    public ChampionController(ILogger<ChampionController> logger,
    ChampionService championService)
    {
        _logger = logger;
        this.championService = championService;
    }

    [HttpGet("ChampionWinrate", Name = "GetChampionWinrate")]
    public async Task<IActionResult> GetChampionWinrate([FromBody] Champion champion)
    {
        return Ok(await championService.GetChampionWinrate(champion));
    }

    [HttpGet("ChampionItems", Name = "GetChampionItems")]

    public async Task<IActionResult> GetChampionWinrateFromItems([FromBody] ChampionItemStatisticsRequest request)
    {
        var results = await championService.GetSimilarWinrates(request.MainChampion, request.items);
        return Ok(results);
    }
}
