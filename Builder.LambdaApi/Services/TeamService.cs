using Builder.Data;
using Builder.Common.Dtos.LambdaApi;
using Builder.Data.Entities;
using Builder.Common.Enums;
using Builder.Cli.Services;
using Builder.LambdaApi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Builder.LambdaApi.Services;

public class TeamService
{
    protected readonly StatisticsDbContext dbContext;

    public TeamService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<TeamCompEntity> ChampionBaseQuery() =>
        dbContext.TeamComps;

    // public async Task<decimal> GetTeamCompAveragePlacement(TeamRequest request)
    // {

    // }
}