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
    protected readonly StatisticsDbContext dbContext;

    public TeamService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<ChampionEntity> ChampionBaseQuery() =>
        dbContext.ChampionEntities;

    public IQueryable<TeamCompEntity> TeamCompBaseQuery() =>
        dbContext.TeamComps;

    public IQueryable<TeamCompChampionJoinEntity> TeamCompChampionJoinBaseQuery() =>
        dbContext.TeamCompChampions;

    public async Task<decimal?> GetTeamCompAveragePlacement(TeamRequest request)
    {
        var hash = CalculateTeamCompHash(request.Champions);
        var teamComp = await TeamCompBaseQuery().Where(x => x.ContentHash == hash).FirstOrDefaultAsync();
        return teamComp?.AveragePlacement;
    }

    public async Task<List<List<ChampionEntity>>> GetTeamCompAlternatives(TeamRequest initialTeam, List<ChampionRequest> alternativeChampions)
    {
        int listLength = initialTeam.Level;

        var alternativeTeamsBy2 = CombinationGenerator.GenerateListCombinationsWithDistance<ChampionRequest>(initialTeam.Champions, alternativeChampions, 2);
        var alternativeTeamsBy1 = CombinationGenerator.GenerateListCombinationsWithDistance<ChampionRequest>(initialTeam.Champions, alternativeChampions, 1);

        var alternativeTeamBy2Hashes = alternativeTeamsBy2.Select(x => CalculateTeamCompHash(x)).ToList();
        var alternativeTeamsBy1Hashes = alternativeTeamsBy1.Select(x => CalculateTeamCompHash(x)).ToList();

        var alternativeTeams = await TeamCompBaseQuery()
            .Where(x => alternativeTeamBy2Hashes.Contains(x.ContentHash) || alternativeTeamsBy1Hashes.Contains(x.ContentHash))
            .OrderBy(x => x.AveragePlacement)
            .Take(5)
            .ToListAsync();

        List<List<ChampionEntity>> otherChampions = new List<List<ChampionEntity>>();

        foreach (var alternativeTeam in alternativeTeams)
        {
            var alternativeTeamChampionsIds = await TeamCompChampionJoinBaseQuery()
                .Where(x => x.TeamCompId == alternativeTeam.TeamCompId)
                .Select(x => x.ChampionEntityId)
                .ToListAsync();

            var alternativeTeamChampions = await ChampionBaseQuery()
                .Where(x => alternativeTeamChampionsIds.Contains(x.ChampionEntityId))
                .ToListAsync();

            otherChampions.Add(alternativeTeamChampions);
        }

        return otherChampions;
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