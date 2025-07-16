using System.Net.Http.Json;
using Builder.Common.Dtos.RiotApi;
using Builder.Data;
using Builder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Builder.Cli.Services;

public class MatchDataRequestService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _riotApiKey;
    private static readonly string requestPath = "/tft/match/v1/matches/{matchId}";

    public MatchDataRequestService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://americas.api.riotgames.com");
        _riotApiKey = _configuration["ApiKeys:RiotApiKey"] ?? throw new InvalidOperationException("Cannot find the Riot games API key when trying to initialize Match ID service.");
        _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _riotApiKey);
    }

    public async Task<Match?> GetMatchData(string matchId)
    {
        string adjustedPath = requestPath.Replace("{matchId}", matchId);
        try
        {
            var results = await _httpClient.GetFromJsonAsync<Match>(adjustedPath);

            if (results == null)
                throw new Exception($"Failed to successfully get match data from matchId {matchId}");
            return results;
        }
        catch (HttpRequestException ex)
        {
            return null;
        }
    }
}