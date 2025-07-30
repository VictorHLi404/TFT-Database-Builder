using Microsoft.Extensions.Configuration;

namespace Builder.LambdaApi.Helpers;

public static class ConfigurationHelper
{
    private static IConfiguration _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static int MinimumInstanceCount => int.TryParse(_configuration["Settings:MinimumInstanceCount"], out var v) ? v : 10;
    public static int SetNumber => int.TryParse(_configuration["Settings:SetNumber"], out var v) ? v : throw new Exception("Set number was not provided in the configuration.");
}