using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Builder.Common.Dtos.RiotApi;
using Builder.Data;
using Builder.Data.Entities;
using Builder.Common.Enums;
using Builder.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Builder.Common.Models.Hashes;

namespace Builder.Cli.Services;

public class DataService
{
    protected readonly StatisticsDbContext dbContext;
    public DataService(StatisticsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IQueryable<ChampionEntity> ChampionBaseQuery() =>
        dbContext.ChampionEntities;

    public IQueryable<TeamCompEntity> TeamCompBaseQuery() =>
        dbContext.TeamComps;

    public IQueryable<TeamCompChampionJoinEntity> TeamCompChampionJoinBaseQuery() =>
        dbContext.TeamCompChampions;

    public Task UpdateChampionEntity(ChampionEntity championEntity)
    {
        return Task.CompletedTask;
    }

    public Task UpdateTeamComp(TeamCompEntity teamCompEntity)
    {
        return Task.CompletedTask;
    }

    public async Task<ChampionEntity?> AddChampionEntity(Unit championDtos, int newPlacement)
    {
        var cleanedChampionName = ProcessingHelper.CleanChampionName(championDtos.character_id);
        if (cleanedChampionName == null)
        {
            return null;
        }
        string itemNames = ProcessingHelper.GetItemString(championDtos.itemNames);
        ChampionEnum champion;
        if (!ChampionEnum.TryParse(cleanedChampionName, true, out champion))
        {
            throw new Exception($"Failed to parse {cleanedChampionName}");
        }
        string hashed = CalculateChampionHash(championDtos);

        ChampionEntity? championEntity = await ChampionBaseQuery().Where(t => t.ContentHash == hashed).FirstOrDefaultAsync()
                                        ?? dbContext.ChampionEntities.Local.FirstOrDefault(t => t.ContentHash == hashed);
        Guid championGuid;

        if (championEntity != null)
        {
            decimal formerAveragePlacement = championEntity.AveragePlacement;
            decimal newAveragePlacement = ((formerAveragePlacement * championEntity.TotalInstances) + newPlacement) / (championEntity.TotalInstances + 1);
            championEntity.AveragePlacement = newAveragePlacement;
            championEntity.TotalInstances = championEntity.TotalInstances + 1;
            championGuid = championEntity.ChampionEntityId;
            return championEntity;
        }
        else
        {
            championGuid = Guid.NewGuid();
            var newChampionEntity = new ChampionEntity
            {
                ChampionEntityId = championGuid,
                Champion = champion,
                ChampionLevel = championDtos.tier,
                Items = itemNames,
                AveragePlacement = newPlacement,
                TotalInstances = 1,
                ContentHash = hashed
            };
            await dbContext.ChampionEntities.AddAsync(newChampionEntity);
            return newChampionEntity;
        }
    }

    public async Task<TeamCompEntity> AddTeamComp(Participant teamCompDtos)
    {
        string hash = CalculateTeamCompHash(teamCompDtos);

        TeamCompEntity? teamCompEntity = await TeamCompBaseQuery().Where(t => t.ContentHash == hash).FirstOrDefaultAsync()
                                        ?? dbContext.TeamComps.Local.FirstOrDefault(t => t.ContentHash == hash);
        Guid teamCompGuid;
        if (teamCompEntity != null)
        {
            decimal formerAveragePlacement = teamCompEntity.AveragePlacement;
            decimal newAveragePlacement = ((formerAveragePlacement * teamCompEntity.TotalInstances) + teamCompDtos.placement) / (teamCompEntity.TotalInstances + 1);
            teamCompEntity.AveragePlacement = newAveragePlacement;
            teamCompEntity.TotalInstances = teamCompEntity.TotalInstances + 1;
            teamCompGuid = teamCompEntity.TeamCompId;
            return teamCompEntity;
        }
        else
        {
            List<string> championHashes = new List<string>();
            foreach (Unit unit in teamCompDtos.units)
            {
                var cleanedChampionName = ProcessingHelper.CleanChampionName(unit.character_id);
                if (cleanedChampionName == null)
                {
                    continue;
                }
                championHashes.Add(CalculateWeakChampionHash(unit));
            }
            teamCompGuid = Guid.NewGuid();
            var newTeamCompEntity = new TeamCompEntity
            {
                TeamCompId = teamCompGuid,
                ContentHash = hash,
                AveragePlacement = teamCompDtos.placement,
                TotalInstances = 1,
                ChampionHashes = championHashes,
            };
            await dbContext.TeamComps.AddAsync(newTeamCompEntity);
            return newTeamCompEntity;
        }
    }

    public async Task AddMatch(Match match)
    {
        foreach (Participant participant in match.info.participants)
        {
            List<ChampionEntity> champions = new List<ChampionEntity>();
            foreach (Unit unit in participant.units)
            {
                ChampionEntity? championEntity = await AddChampionEntity(unit, participant.placement);
                if (championEntity != null)
                {
                    champions.Add(championEntity);
                }
            }
            TeamCompEntity teamCompEntity = await AddTeamComp(participant);
            foreach (ChampionEntity champion in champions)
            {
                TeamCompChampionJoinEntity? join = await TeamCompChampionJoinBaseQuery()
                                                    .Where(t => t.TeamCompId == teamCompEntity.TeamCompId
                                                            && t.ChampionEntityId == champion.ChampionEntityId)
                                                    .FirstOrDefaultAsync()
                                                    ?? dbContext.TeamCompChampions.Local.Where(t => t.TeamCompId == teamCompEntity.TeamCompId
                                                            && t.ChampionEntityId == champion.ChampionEntityId)
                                                    .FirstOrDefault();
                if (join == null)
                {
                    TeamCompChampionJoinEntity newJoin = new TeamCompChampionJoinEntity
                    {
                        TeamCompId = teamCompEntity.TeamCompId,
                        ChampionEntityId = champion.ChampionEntityId,
                        TeamComp = teamCompEntity,
                        Champion = champion,
                    };
                    await dbContext.TeamCompChampions.AddAsync(newJoin);
                }
            }
        }
        await dbContext.SaveChangesAsync();
        return;
    }

    private string CalculateChampionHash(Unit champion)
    {
        return HashHelper.CalculateChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.character_id) ?? throw new Exception("Cannot calculate champion hash of an invalid champion"),
            Items = ProcessingHelper.GetItemString(champion.itemNames),
            Level = champion.tier
        });
    }

    private string CalculateWeakChampionHash(Unit champion)
    {
        return HashHelper.CalculateWeakChampionHash(new()
        {
            ChampionName = ProcessingHelper.CleanChampionName(champion.character_id) ?? throw new Exception("Cannot calculate champion hash of an invalid champion"),
            Level = champion.tier
        });
    }

    public static string CalculateTeamCompHash(Participant participantDtos)
    {
        var sortedChampions = participantDtos.units.OrderBy(t => t.character_id)
            .ThenBy(t => ProcessingHelper.GetItemString(t.itemNames))
            .Where(t => ProcessingHelper.CleanChampionName(t.character_id) != null)
            .ToList();

        var championHashRequests = sortedChampions.Select(x => new WeakChampionHashCreateModel
        {
            ChampionName = ProcessingHelper.CleanChampionName(x.character_id) ?? throw new Exception("Cannot calculate champion hash of an invalid champion"),
            Level = x.tier
        }).ToList();
        return HashHelper.CalculateTeamCompHash(new()
        {
           ChampionHashRequests = championHashRequests 
        });
    }
}