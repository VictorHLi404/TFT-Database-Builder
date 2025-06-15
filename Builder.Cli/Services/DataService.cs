using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Builder.Cli.Dtos;
using Builder.Data;
using Builder.Data.Entities;
using Builder.Data.Enums;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Guid?> AddChampionEntity(Unit championDtos, int newPlacement)
    {
        string cleanedChampionName = CleanChampionName(championDtos.character_id);
        if (cleanedChampionName == null)
        {
            return null;
        }
        string itemNames = GetItemString(championDtos.itemNames);
        ChampionEnum champion;
        if (!ChampionEnum.TryParse(cleanedChampionName, true, out champion))
        {
            throw new Exception($"Failed to parse {cleanedChampionName}");
        }
        Console.WriteLine($"THIS IS THE CHAMPION: {champion}");
        string hashed = CalculateChampionHash(cleanedChampionName, itemNames, championDtos);

        ChampionEntity? championEntity = await ChampionBaseQuery().Where(t => t.ContentHash == hashed).FirstOrDefaultAsync()
                                        ?? dbContext.ChampionEntities.Local.FirstOrDefault(t => t.ContentHash == hashed);
        Guid championGuid;

        if (championEntity != null)
        {
            Console.WriteLine($"HAVE ALREADY SEEN {cleanedChampionName}, {itemNames}, {championDtos.tier}");
            decimal formerAveragePlacement = championEntity.AveragePlacement;
            decimal newAveragePlacement = ((formerAveragePlacement * championEntity.TotalInstances) + newPlacement) / (championEntity.TotalInstances + 1);
            championEntity.AveragePlacement = newAveragePlacement;
            championEntity.TotalInstances = championEntity.TotalInstances + 1;
            championGuid = championEntity.ChampionEntityId;
        }
        else
        {
            Console.WriteLine($"NEW CHAMPION {cleanedChampionName}, {itemNames}, {championDtos.tier}");
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
        }
        // await dbContext.SaveChangesAsync();
        return championGuid;
    }

    public async Task<Guid> AddTeamComp(Participant teamCompDtos)
    {
        string hash = CalculateTeamCompHash(teamCompDtos);

        TeamCompEntity? teamCompEntity = await TeamCompBaseQuery().Where(t => t.ContentHash == hash).FirstOrDefaultAsync()
                                        ?? dbContext.TeamComps.Local.FirstOrDefault( t => t.ContentHash == hash);
        Guid teamCompGuid;
        if (teamCompEntity != null)
        {
            Console.WriteLine($"HAVE ALREADY SEEN TEAM COMP {hash}");
            decimal formerAveragePlacement = teamCompEntity.AveragePlacement;
            decimal newAveragePlacement = ((formerAveragePlacement * teamCompEntity.TotalInstances) + teamCompDtos.placement) / (teamCompEntity.TotalInstances + 1);
            teamCompEntity.AveragePlacement = newAveragePlacement;
            teamCompEntity.TotalInstances = teamCompEntity.TotalInstances + 1;
            teamCompGuid = teamCompEntity.TeamCompId;
        }
        else
        {
            Console.WriteLine($"NEW TEAM COMP {hash}");
            List<string> championHashes = new List<string>();
            foreach (Unit unit in teamCompDtos.units)
            {
                string cleanedChampionName = CleanChampionName(unit.character_id);
                if (cleanedChampionName == null)
                {
                    continue;
                }
                championHashes.Add(CalculateWeakChampionHash(cleanedChampionName, unit));
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
        }
        // await dbContext.SaveChangesAsync();
        return teamCompGuid;
    }

    public async Task AddMatch(Match match)
    {
        foreach (Participant participant in match.info.participants)
        {
            List<Guid> unitIds = new List<Guid>();
            foreach (Unit unit in participant.units)
            {
                Guid? championGuid = await AddChampionEntity(unit, participant.placement);
                if (championGuid != null)
                {
                    unitIds.Add(championGuid.Value);
                }
            }
            Guid teamCompGuid = await AddTeamComp(participant);
        }
        await dbContext.SaveChangesAsync();
        return;
    }

    public string? CleanChampionName(string championName)
    {
        string newName = championName.Replace("TFT14_", "").Replace(" ", "").Replace("'", "");
        if (newName.Contains("Summon"))
        {
            return null;
        }
        if (newName.Contains("NidaleeCougar"))
        {
            newName = newName.Replace("NidaleeCougar", "Nidalee");
        }
        else if (newName.Contains("Jarvan"))
        {
            newName = newName.Replace("Jarvan", "JarvanIV");
        }
        return newName;
    }

    public string GetItemString(List<string> items)
    {
        items.Sort();
        return string.Join("-", items).Replace(" ", "").Replace("'", "");
    }

    public string CalculateChampionHash(string cleanedChampionName, string items, Unit championDtos)
    {
        string contentToHash = $"{cleanedChampionName}-{items}-{championDtos.tier}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public string CalculateWeakChampionHash(string cleanedChampionName, Unit championDtos)
    {
        string contentToHash = $"{cleanedChampionName}-{championDtos.tier}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        } 
    }

    public string CalculateTeamCompHash(Participant participantDtos)
    {
        string contentToHash = "";
        List<Unit> sortedChampions = participantDtos.units.OrderBy(t => t.character_id)
            .ThenBy(t =>
            {
            var sortedItemNames = t.itemNames.OrderBy(item => item).ToList();
            return string.Join("-", sortedItemNames).Replace(" ", "").Replace("'", "");
            })
            .ToList();
        foreach (Unit unit in sortedChampions)
        {
            string cleanedChampionName = CleanChampionName(unit.character_id);
            if (cleanedChampionName == null) continue;
            contentToHash += CalculateWeakChampionHash(cleanedChampionName, unit);
        }
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }


    

    

}