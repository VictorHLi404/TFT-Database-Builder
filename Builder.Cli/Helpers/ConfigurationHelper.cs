using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Builder.Cli.Helpers;

public static class ConfigurationHelper
{
    private static IConfiguration _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static int MinimumInstanceCount => int.TryParse(_configuration["Settings:MinimumInstanceCount"], out var v) ? v : 100;
    public static int RequestTimeout => int.TryParse(_configuration["Settings:RequestTimeout"], out var v) ? v : 7500;
    public static int MaxGames => int.TryParse(_configuration["Settings:MaxGames"], out var v) ? v : 20000;
    public static int BFSQueueMaxSize => int.TryParse(_configuration["Settings:BFSQueueMaxSize"], out var v) ? v : 100;
    
    public static int SetNumber => int.TryParse(_configuration["Settings:SetNumber"], out var v) ? v : throw new Exception("Set number was not provided in the configuration.");
    public static string GameType => _configuration["Settings:GameType"] ?? throw new Exception("Game type was not provided in the configuration.");
    public static int QueueId => int.TryParse(_configuration["Settings:QueueId"], out var v) ? v : throw new Exception("Queue Id was not provided in the configuration.");
}