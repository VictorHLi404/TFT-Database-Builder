using Builder.LambdaApi.Dtos;
using Builder.LambdaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Builder.LambdaApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<TestController> _logger;
    private readonly ChampionService championService;

    public TestController(ILogger<TestController> logger,
    ChampionService championService)
    {
        _logger = logger;
        this.championService = championService;
    }

    [HttpGet("NewEndpoint", Name = "TestFunctionAhhGuy")]
    public async Task<IActionResult> TestFunctionAhhGuy([FromQuery] string name)
    {
        return Ok(await championService.testFunction(name));
        // return Ok($"{payload.UserName} is {payload.Age} years old.");
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
