using Builder.Data;
using Builder.Common.Dtos.LambdaApi.Champion;
using Builder.Data.Entities;
using Builder.Common.Enums;
using Builder.Common.Helpers;
using Builder.LambdaApi.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Builder.Common.Constants;

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

    public async Task<List<ChampionEntity>> GetSimilarWinrates(ChampionRequest originalChampion, List<List<ItemEnum>> possibleItemSets)
    {
        // to compensate for current lack of data, also query the level from the current champion down.
        ChampionRequest downLevelledChampion = new ChampionRequest
        {
            ChampionName = originalChampion.ChampionName,
            Items = originalChampion.Items,
            Level = originalChampion.Level - 1
        };
        List<string> hashes = new List<string>();

        foreach (var itemSet in possibleItemSets)
        {
            hashes.AddRange(GetPossibleItemSetHashes(originalChampion, itemSet));
            if (originalChampion.Level > 1)
                hashes.AddRange(GetPossibleItemSetHashes(downLevelledChampion, itemSet));

            hashes = hashes.Distinct().ToList();
        }

        var champions = await ChampionBaseQuery().Where(t => hashes.Contains(t.ContentHash) && t.TotalInstances >= ConstantValues.MINIMUM_GAMES_PLAYED)
                        .OrderBy(t => t.AveragePlacement)
                        .Take(5)
                        .ToListAsync();

        return champions;
    }

    private string CalculateChampionHash(ChampionRequest champion, List<string> items)
    {
        return HashHelper.CalculateChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.ChampionName.ToString()) ?? throw new Exception("Could not parse the champion name provided"),
            Items = ProcessingHelper.GetItemString(items),
            Level = champion.Level
        });
    }

    private string CalculateChampionHash(ChampionRequest champion)
    {
        return HashHelper.CalculateChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.ChampionName.ToString()) ?? throw new Exception("Could not parse the champion name provided"),
            Items = ProcessingHelper.GetItemString(champion.Items.Select(x => x.ToString()).ToList()),
            Level = champion.Level
        });
    }
    private List<string> GetPossibleItemSetHashes(ChampionRequest champion, List<ItemEnum> items)
    {
        int listLength = items.Count;
        List<List<int>> subsetsOf2 = CombinationGenerator.GetSubsets(listLength, 2);
        List<List<int>> subsetsOf3 = CombinationGenerator.GetSubsets(listLength, 3);
        List<List<string>> twoItemNames = subsetsOf2.Select(subset =>
                                        new List<string> {items[subset[0]].ToString(),
                                                            items[subset[1]].ToString() }
                                                            .OrderBy(s => s).ToList())
                                                            .ToList();
        List<string> hashes = twoItemNames.Select(itemNames => CalculateChampionHash(champion, itemNames)).ToList();

        List<List<string>> threeItemNames = subsetsOf3.Select(subset =>
                                                new List<string> { items[subset[0]].ToString(),
                                                    items[subset[1]].ToString(),
                                                    items[subset[2]].ToString() }
                                                    .OrderBy(s => s).ToList())
                                                    .ToList();

        hashes.AddRange(threeItemNames.Select(itemNames => CalculateChampionHash(champion, itemNames)).ToList());

        return hashes;
    }
}