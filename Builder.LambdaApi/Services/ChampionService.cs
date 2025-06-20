using Builder.Data;
using Builder.LambdaApi.Dtos;
using Builder.Data.Entities;
using Builder.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Builder.LambdaApi.Services;

public class ChampionService
{
    protected readonly StatisticsDbContext dbContext;

    public ChampionService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<ChampionEntity> ChampionBaseQuery() =>
        dbContext.ChampionEntities;

    public async Task<int> testFunction(string name)
    {
        ChampionEnum champion;
        if (!ChampionEnum.TryParse(name, true, out champion))
        {
            throw new Exception($"Failed to parse {name}");
        }
        return await ChampionBaseQuery().Where(t => t.Champion == champion).CountAsync();
    }
}