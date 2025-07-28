using Builder.Data;
using Builder.Common.Dtos.LambdaApi.Champion;
using Builder.Data.Entities;
using Builder.Common.Helpers;
using Builder.LambdaApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Builder.Common.Constants;

namespace Builder.LambdaApi.Services;

public class BaseDataService
{
    protected readonly StatisticsDbContext dbContext;
    public BaseDataService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<ChampionEntity> ChampionBaseQuery() =>
        dbContext.ChampionEntities;

    public IQueryable<TeamCompEntity> TeamCompBaseQuery() =>
        dbContext.TeamComps;

    public IQueryable<TeamCompChampionJoinEntity> TeamCompChampionJoinBaseQuery() =>
        dbContext.TeamCompChampions;

    public IQueryable<WeakChampionEntity> WeakChampionBaseQuery() =>
        dbContext.WeakChampionEntities;
}