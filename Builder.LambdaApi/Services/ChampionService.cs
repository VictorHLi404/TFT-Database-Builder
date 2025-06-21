using Builder.Data;
using Builder.LambdaApi.Dtos;
using Builder.Data.Entities;
using Builder.Data.Enums;
using Builder.Cli.Services;
using Builder.LambdaApi.Helpers;
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

    public async Task<decimal> GetChampionWinrate(Champion champion)
    {
        string itemNames = string.Join("-", champion.Items.Select(item => "TFT_Item_" + item.ToString()).ToList());
        string contentHash = DataService.CalculateChampionHash(champion.ChampionName.ToString(), itemNames, champion.Level);
        return await ChampionBaseQuery().Where(t => t.ContentHash == contentHash).Select(t => t.AveragePlacement).FirstOrDefaultAsync();
    }

    public async Task<List<ChampionEntity>> GetSimilarWinrates(Champion originalChampion, List<ItemEnum> items)
    {
        int listLength = items.Count;
        List<List<int>> subsetsOf2 = CombiationGenerator.GetSubsets(listLength, 2);
        List<List<int>> subsetsOf3 = CombiationGenerator.GetSubsets(listLength, 3);
        return new List<ChampionEntity>();
    }
}