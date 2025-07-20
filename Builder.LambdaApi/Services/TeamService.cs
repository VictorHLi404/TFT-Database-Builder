using Builder.Data;
using Builder.Common.Dtos.LambdaApi;
using Builder.Data.Entities;
using Builder.Common.Enums;
using Builder.Cli.Services;
using Builder.LambdaApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Builder.Common.Helpers;
using Builder.Common.Models.Hashes;

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
        var hash = CalculateTeamCompHash(request);
        var teamComp = await TeamCompBaseQuery().Where(x => x.ContentHash == hash).FirstOrDefaultAsync();
        return teamComp?.AveragePlacement;
    }

    private string CalculateTeamCompHash(TeamRequest request)
    {
        var sortedChampions = request.Champions.OrderBy(t => t.ChampionName.ToString())
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