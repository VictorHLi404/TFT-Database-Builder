using System.Net.Http.Json;
using Builder.Cli.Dtos;
using Builder.Data;
using Builder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Builder.Cli.Services;

public class MatchIDRequestService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _riotApiKey;
    private static readonly string requestPath = "/tft/match/v1/matches/by-puuid/{puuid}/ids";

    public MatchIDRequestService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://americas.api.riotgames.com");
        _riotApiKey = _configuration["ApiKeys:RiotApiKey"] ?? throw new InvalidOperationException("Cannot find the Riot games API key when trying to initialize Match ID service.");
        _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _riotApiKey);
    }

    public async Task<List<string>> getMatchIDs(string puuid)
    {
        string adjustedPath = requestPath.Replace("{puuid}", puuid);
        try
        {
            var results = await _httpClient.GetFromJsonAsync<List<string>>(adjustedPath);

            if (results == null)
                throw new Exception($"Failed to successfully get MatchIDs from PUUID {puuid}.");

            return results;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API Request failed: {ex.Message}");
            return [];
        }

    }
}