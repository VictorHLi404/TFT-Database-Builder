using Builder.Common.Dtos.LambdaApi.Champion;
using Builder.Common.Helpers;
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

    [HttpPost("ChampionWinrate", Name = "GetChampionAveragePlacement")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChampionAveragePlacement([FromBody] ChampionRequest champion)
    {
        return Ok(await championService.GetChampionAveragePlacement(champion));
    }

    [HttpPost("ChampionItems", Name = "GetChampionItems")]
    [ProducesResponseType(typeof(List<ChampionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChampionAveragePlacementFromItems([FromBody] ChampionItemStatisticsRequest request)
    {
        var results = await championService.GetSimilarWinrates(request.MainChampion, request.PossibleItemSets);

        var response = results.Select(x => new ChampionResponse
        {
            ChampionName = x.Champion,
            AveragePlacement = x.AveragePlacement,
            Items = x.Items != null ? ProcessingHelper.GetItemEnums(x.Items) : [],
            Level = x.ChampionLevel,
        }).ToList();

        return Ok(response);
    }
}
