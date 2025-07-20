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

    public async Task<ChampionEntity?> AddChampionEntity(
        Unit championDto,
        int newPlacement,
        Dictionary<string, ChampionEntity> existingChampionEntities)
    {
        var cleanedChampionName = ProcessingHelper.CleanChampionName(championDto.character_id);
        if (cleanedChampionName == null)
        {
            return null;
        }
        string itemNames = ProcessingHelper.GetItemString(championDto.itemNames);
        ChampionEnum champion;
        if (!ChampionEnum.TryParse(cleanedChampionName, true, out champion))
        {
            throw new Exception($"Failed to parse {cleanedChampionName}");
        }

        string hash = CalculateChampionHash(championDto);
        ChampionEntity? championEntity = null;

        if (existingChampionEntities.TryGetValue(hash, out championEntity))
            return UpdateChampionStatistics(championEntity, newPlacement);

        championEntity = dbContext.ChampionEntities.Local.FirstOrDefault(t => t.ContentHash == hash);

        if (championEntity != null)
            return UpdateChampionStatistics(championEntity, newPlacement);

        else
        {
            var newChampionEntity = new ChampionEntity
            {
                ChampionEntityId = Guid.NewGuid(),
                Champion = champion,
                ChampionLevel = championDto.tier,
                Items = itemNames,
                AveragePlacement = newPlacement,
                TotalInstances = 1,
                ContentHash = hash
            };
            await dbContext.ChampionEntities.AddAsync(newChampionEntity);
            existingChampionEntities.Add(hash, newChampionEntity);
            return newChampionEntity;
        }
    }

    public async Task<TeamCompEntity> AddTeamComp(
        Participant teamCompDtos,
        Dictionary<string, TeamCompEntity> existingTeamCompEntities)
    {
        string hash = CalculateTeamCompHash(teamCompDtos);
        TeamCompEntity? teamCompEntity = null;

        if (existingTeamCompEntities.TryGetValue(hash, out teamCompEntity))
        {
            return UpdateTeamCompStatistics(teamCompEntity, teamCompDtos.placement);
        }

        teamCompEntity = dbContext.TeamComps.Local.FirstOrDefault(t => t.ContentHash == hash);

        if (teamCompEntity != null)
        {
            return UpdateTeamCompStatistics(teamCompEntity, teamCompDtos.placement);
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

            var newTeamCompEntity = new TeamCompEntity
            {
                TeamCompId = Guid.NewGuid(),
                ContentHash = hash,
                AveragePlacement = teamCompDtos.placement,
                TotalInstances = 1,
                ChampionHashes = championHashes,
            };
            await dbContext.TeamComps.AddAsync(newTeamCompEntity);
            existingTeamCompEntities.Add(hash, newTeamCompEntity);
            return newTeamCompEntity;
        }
    }

    public async Task AddMatch(Match match)
    {
        var currentMatchChampionHashes = new HashSet<string>();
        var currentMatchTeamCompHashes = new HashSet<string>();
        var potentialJoinKeys = new HashSet<(Guid teamCompId, Guid championEntityId)>();

        foreach (Participant participant in match.info.participants)
        {
            currentMatchTeamCompHashes.Add(CalculateTeamCompHash(participant));
            foreach (Unit unit in participant.units)
            {
                if (ProcessingHelper.CleanChampionName(unit.character_id) != null)
                    currentMatchChampionHashes.Add(CalculateChampionHash(unit));
            }
        }

        var existingChampionEntities = await ChampionBaseQuery()
            .Where(c => currentMatchChampionHashes.Contains(c.ContentHash))
            .ToDictionaryAsync(c => c.ContentHash, c => c) ?? new Dictionary<string, ChampionEntity>();

        var existingTeamCompEntities = await TeamCompBaseQuery()
            .Where(c => currentMatchTeamCompHashes.Contains(c.ContentHash))
            .ToDictionaryAsync(c => c.ContentHash, c => c) ?? new Dictionary<string, TeamCompEntity>();

        foreach (Participant participant in match.info.participants)
        {
            TeamCompEntity teamCompEntity = await AddTeamComp(participant, existingTeamCompEntities);
            List<ChampionEntity> champions = new List<ChampionEntity>();
            foreach (Unit unit in participant.units)
            {
                ChampionEntity? championEntity = await AddChampionEntity(unit, participant.placement, existingChampionEntities);
                if (championEntity != null)
                {
                    champions.Add(championEntity);
                }
            }

            foreach (ChampionEntity champion in champions)
            {
                potentialJoinKeys.Add((teamCompEntity.TeamCompId, champion.ChampionEntityId));
                // TeamCompChampionJoinEntity? join = await TeamCompChampionJoinBaseQuery()
                //                                     .Where(t => t.TeamCompId == teamCompEntity.TeamCompId
                //                                             && t.ChampionEntityId == champion.ChampionEntityId)
                //                                     .FirstOrDefaultAsync()
                //                                     ?? dbContext.TeamCompChampions.Local.Where(t => t.TeamCompId == teamCompEntity.TeamCompId
                //                                             && t.ChampionEntityId == champion.ChampionEntityId)
                //                                     .FirstOrDefault();
                // if (join == null)
                // {
                //     TeamCompChampionJoinEntity newJoin = new TeamCompChampionJoinEntity
                //     {
                //         TeamCompId = teamCompEntity.TeamCompId,
                //         ChampionEntityId = champion.ChampionEntityId,
                //         TeamComp = teamCompEntity,
                //         Champion = champion,
                //     };
                //     await dbContext.TeamCompChampions.AddAsync(newJoin);
                // }
            }
        }
        var teamCompIdsInCurrentContext = dbContext.ChangeTracker.Entries<TeamCompEntity>()
                                        .Where(e => e.State == EntityState.Added || e.State == EntityState.Unchanged || e.State == EntityState.Modified)
                                        .Select(e => e.Entity.TeamCompId)
                                        .ToHashSet();

        var existingJoins = await TeamCompChampionJoinBaseQuery()
            .Where(j => teamCompIdsInCurrentContext.Contains(j.TeamCompId))
            .ToListAsync();

        var existingJoinSet = new HashSet<(Guid teamCompId, Guid championId)>();
        foreach (var join in existingJoins)
        {
            existingJoinSet.Add((join.TeamCompId, join.ChampionEntityId));
        }

        foreach (var (teamCompId, championId) in potentialJoinKeys)
        {
            if (!existingJoinSet.Contains((teamCompId, championId)))
            {
                // Find the actual entities from the context's change tracker or existing dictionaries
                // This is important for EF Core to correctly set up the relationships for new joins
                var teamComp = dbContext.TeamComps.Local.FirstOrDefault(tc => tc.TeamCompId == teamCompId)
                               ?? existingTeamCompEntities.Values.FirstOrDefault(tc => tc.TeamCompId == teamCompId);

                var champion = dbContext.ChampionEntities.Local.FirstOrDefault(c => c.ChampionEntityId == championId)
                               ?? existingChampionEntities.Values.FirstOrDefault(c => c.ChampionEntityId == championId);

                if (teamComp != null && champion != null)
                {
                    TeamCompChampionJoinEntity newJoin = new TeamCompChampionJoinEntity
                    {
                        TeamCompId = teamCompId,
                        ChampionEntityId = championId,
                        TeamComp = teamComp,
                        Champion = champion,
                    };
                    dbContext.TeamCompChampions.Add(newJoin);
                }
                else
                {
                    Console.WriteLine($"Warning: Could not find TeamComp or Champion for join: TeamCompId={teamCompId}, ChampionId={championId}");
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

    private string CalculateTeamCompHash(Participant participantDtos)
    {
        var sortedChampions = participantDtos.units.OrderBy(t => t.character_id)
            .ThenBy(t => ProcessingHelper.GetItemString(t.itemNames))
            .Where(t => ProcessingHelper.CleanChampionName(t.character_id) != null)
            .ToList();

        var championHashRequests = sortedChampions.Select(x => new WeakChampionHashCreateModel
        {
            ChampionName = ProcessingHelper.CleanChampionName(x.character_id)!,
            Level = x.tier
        }).ToList();

        return HashHelper.CalculateTeamCompHash(new()
        {
            ChampionHashRequests = championHashRequests
        });
    }

    private TeamCompEntity UpdateTeamCompStatistics(TeamCompEntity teamCompEntity, int newPlacement)
    {
        decimal formerAveragePlacement = teamCompEntity.AveragePlacement;
        decimal newAveragePlacement = ((formerAveragePlacement * teamCompEntity.TotalInstances) + newPlacement) / (teamCompEntity.TotalInstances + 1);
        teamCompEntity.AveragePlacement = newAveragePlacement;
        teamCompEntity.TotalInstances = teamCompEntity.TotalInstances + 1;
        return teamCompEntity;
    }

    private ChampionEntity UpdateChampionStatistics(ChampionEntity championEntity, int newPlacement)
    {
        decimal formerAveragePlacement = championEntity.AveragePlacement;
        decimal newAveragePlacement = ((formerAveragePlacement * championEntity.TotalInstances) + newPlacement) / (championEntity.TotalInstances + 1);
        championEntity.AveragePlacement = newAveragePlacement;
        championEntity.TotalInstances = championEntity.TotalInstances + 1;
        return championEntity;
    }
}