using Builder.Data;
using Builder.Common.Dtos.LambdaApi.Team;
using Builder.Common.Dtos.LambdaApi.Champion;
using Builder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Builder.Common.Helpers;
using Builder.Common.Models.Hashes;
using Builder.LambdaApi.Helpers;

namespace Builder.LambdaApi.Services;

public class TeamService
{
    protected readonly BaseDataService dataService;

    public TeamService(BaseDataService dataService)
    {
        this.dataService = dataService;
    }

    public async Task<(decimal placement, List<WeakChampionEntity> champions)> GetTeamCompAveragePlacement(TeamRequest request)
    {
        var hash = CalculateTeamCompHash(request.Champions);
        var teamComp = await dataService.TeamCompBaseQuery().Where(x => x.ContentHash == hash).FirstOrDefaultAsync();
        if (teamComp == null)
            return (0, []);
        return await GetWeakChampions(teamComp);
    }

    public async Task<List<(decimal placement, List<WeakChampionEntity> champions)>> GetTeamCompAlternatives(TeamRequest initialTeam, List<ChampionRequest> alternativeChampions)
    {
        int listLength = initialTeam.Level;

        var alternativeTeamsBy2 = CombinationGenerator.GenerateListCombinationsWithDistance<ChampionRequest>(initialTeam.Champions, alternativeChampions, 2);
        var alternativeTeamsBy1 = CombinationGenerator.GenerateListCombinationsWithDistance<ChampionRequest>(initialTeam.Champions, alternativeChampions, 1);

        var alternativeTeamBy2Hashes = alternativeTeamsBy2.Select(x => CalculateTeamCompHash(x)).ToList();
        var alternativeTeamsBy1Hashes = alternativeTeamsBy1.Select(x => CalculateTeamCompHash(x)).ToList();

        var alternativeTeams = await dataService.TeamCompBaseQuery()
            .Where(x => alternativeTeamBy2Hashes.Contains(x.ContentHash) || alternativeTeamsBy1Hashes.Contains(x.ContentHash))
            .OrderBy(x => x.AveragePlacement)
            .Take(5)
            .ToListAsync();


        var alternativeTeamDetails = new List<(decimal placement, List<WeakChampionEntity> champions)>();

        foreach (var alternativeTeam in alternativeTeams)
        {
            alternativeTeamDetails.Add(await GetWeakChampions(alternativeTeam));
        }

        return alternativeTeamDetails;
    }

    public async Task<(decimal placement, List<WeakChampionEntity> champions)> GetWeakChampions(TeamCompEntity teamComp)
    {
        var hashList = teamComp.ChampionHashes!.ToList();
        var champions = await dataService.WeakChampionBaseQuery().Where(x => hashList.Contains(x.ContentHash)).ToListAsync();
        return (teamComp.AveragePlacement, champions);
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