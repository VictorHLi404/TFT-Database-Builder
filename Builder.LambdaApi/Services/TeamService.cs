using Builder.Data;
using Builder.Common.Dtos.LambdaApi;
using Builder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Builder.Common.Helpers;
using Builder.Common.Models.Hashes;
using Builder.LambdaApi.Helpers;

namespace Builder.LambdaApi.Services;

public class TeamService
{
    protected readonly StatisticsDbContext dbContext;

    public TeamService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<TeamCompEntity> TeamCompBaseQuery() =>
        dbContext.TeamComps;

    public async Task<decimal?> GetTeamCompAveragePlacement(TeamRequest request)
    {
        var hash = CalculateTeamCompHash(request.Champions);
        var teamComp = await TeamCompBaseQuery().Where(x => x.ContentHash == hash).FirstOrDefaultAsync();
        return teamComp?.AveragePlacement;
    }

    public async Task<List<TeamCompEntity>> GetTeamCompAlternatives(TeamRequest initialTeam, List<ChampionRequest> alternativeChampions)
    {
        int listLength = initialTeam.Level;

        CombiationGenerator.GenerateListCombinationsWithDistance<ChampionRequest>(initialTeam.Champions, initialTeam.Champions, 2);

        return [];
    }

    private string CalculateTeamCompHash(List<ChampionRequest> request)
    {
        var sortedChampions = request.OrderBy(t => t.ChampionName.ToString())
            .ThenBy(t => ProcessingHelper.GetItemString(t.Items.Select(x => x.ToString()).ToList()))
            .Where(t => ProcessingHelper.CleanChampionName(t.ChampionName.ToString()) != null)
            .ToList();

        var championHashRequests = sortedChampions.Select(x => new WeakChampionHashCreateModel
        {
            ChampionName = ProcessingHelper.CleanChampionName(x.ChampionName.ToString())!,
            Level = x.Level
        }).ToList();

        return HashHelper.CalculateTeamCompHash(new()
        {
            ChampionHashRequests = championHashRequests
        });
    }
}