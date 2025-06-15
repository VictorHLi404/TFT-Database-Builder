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

        ChampionEntity? championEntity = await ChampionBaseQuery().Where(t => t.ContentHash == hashed).FirstOrDefaultAsync();
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
        await dbContext.SaveChangesAsync();
        return championGuid;
    }

    public async Task AddTeamComp(Participant teamCompDtos)
    {
        // string hash = CalculateTeamCompHash(teamCompDtos);
        // TeamCompEntity? teamCompEntity = await TeamCompBaseQuery().Where(t => t.ContentHash == hash).FirstOrDefaultAsync();
        // if (teamCompEntity != null)
        // {

        // }
        // else
        // {
        //     Console.WriteLine($"NEW TEAM COMP {hash}");
        //     var newTeamCompEntity = new TeamCompEntity
        //     {
        //         TeamCompId = Guid.NewGuid(),
        //         ContentHash = hash
        //     }

        // }
        // await dbContext.SaveChangesAsync();
        // return;
        return;
    }

    public Task AddMatch(Match match)
    {



        return Task.CompletedTask;
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

    public string CalculateTeamCompHash(Participant participantDtos)
    {
        string contentToHash = "";
        List<Unit> sortedChampions = participantDtos.units.OrderBy(t => t.character_id).ToList();
        foreach (Unit unit in sortedChampions)
        {
            string cleanedChampionName = CleanChampionName(unit.character_id) ?? throw new Exception("Could not clean the champion name correctly.");
            string items = GetItemString(unit.itemNames);
            string championToHash = $"{cleanedChampionName}-{items}-{unit.tier}-";
            contentToHash += championToHash;
        }
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }


    

    

}