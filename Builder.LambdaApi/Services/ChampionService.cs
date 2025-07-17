using Builder.Data;
using Builder.Common.Dtos.LambdaApi;
using Builder.Data.Entities;
using Builder.Common.Enums;
using Builder.Common.Helpers;
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

    public async Task<decimal> GetChampionAveragePlacement(ChampionRequest champion)
    {
        string contentHash = CalculateChampionHash(champion);
        return await ChampionBaseQuery().Where(t => t.ContentHash == contentHash).Select(t => t.AveragePlacement).FirstOrDefaultAsync();
    }

    public async Task<List<ChampionEntity>> GetSimilarWinrates(ChampionRequest originalChampion, List<ItemEnum> items)
    {
        int listLength = items.Count;
        List<List<int>> subsetsOf2 = CombiationGenerator.GetSubsets(listLength, 2);
        List<List<int>> subsetsOf3 = CombiationGenerator.GetSubsets(listLength, 3);
        List<List<string>> twoItemNames = subsetsOf2.Select(subset =>
                                        new List<string> {items[subset[0]].ToString(),
                                                            items[subset[1]].ToString() }
                                                            .OrderBy(s => s).ToList())
                                        .ToList();
        List<string> twoItemHashes = twoItemNames.Select(itemNames => CalculateChampionHash(originalChampion, itemNames)).ToList();

        var twoItemChampions = await ChampionBaseQuery().Where(t => twoItemHashes.Contains(t.ContentHash)).OrderBy(t => t.AveragePlacement).Take(5).ToListAsync();

        List<List<string>> threeItemNames = subsetsOf3.Select(subset =>
                                                new List<string> { items[subset[0]].ToString(),
                                                    items[subset[1]].ToString(),
                                                    items[subset[2]].ToString() }
                                                    .OrderBy(s => s).ToList())
                                                    .ToList();
        List<string> threeItemHashes = threeItemNames.Select(itemNames => CalculateChampionHash(originalChampion, itemNames)).ToList();

        var threeItemChampions = await ChampionBaseQuery().Where(t => threeItemHashes.Contains(t.ContentHash)).OrderBy(t => t.AveragePlacement).Take(5).ToListAsync();

        return twoItemChampions.Concat(threeItemChampions)
                                .OrderBy(t => t.AveragePlacement)
                                .Take(5)
                                .ToList();
    }

    private string CalculateChampionHash(ChampionRequest champion, List<string> items)
    {
        return HashHelper.CalculateChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.ChampionName.ToString()) ?? throw new Exception("Could not parse the champion name provided"),
            Items = string.Join("-", items),
            Level = champion.Level
        });
    }

    private string CalculateChampionHash(ChampionRequest champion)
    {
        return HashHelper.CalculateChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.ChampionName.ToString()) ?? throw new Exception("Could not parse the champion name provided"),
            Items = string.Join("-", champion.Items.Select(x => x.ToString()).ToList()),
            Level = champion.Level
        });
    }
}