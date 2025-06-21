using Builder.Data.Enums;
using Builder.LambdaApi.Dtos;
using Builder.LambdaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Builder.LambdaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ChampionController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

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

    public async Task<IActionResult> GetChampionItems([FromBody] ChampionItemStatisticsRequest request)
    {
        await championService.GetSimilarWinrates(request.MainChampion, request.items);
        return Ok();
    }

    // [HttpGet(Name = "GetTest")]
    // public IEnumerable<WeatherForecast> Get([FromQuery] string query)
    // {
    //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //     {
    //         Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //         TemperatureC = Random.Shared.Next(-20, 55),
    //         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //     })
    //     .ToArray();
    // }
}
