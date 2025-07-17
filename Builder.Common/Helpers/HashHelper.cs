using System.Security.Cryptography;
using System.Text;
using Builder.Common.Dtos.RiotApi;

namespace Builder.Common.Helpers;

public class HashHelper
{
    public static string CalculateChampionHash(string cleanedChampionName, string items, int tier)
    {
        string contentToHash = $"{cleanedChampionName}-{items}-{tier}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string CalculateWeakChampionHash(string cleanedChampionName, Unit championDtos)
    {
        string contentToHash = $"{cleanedChampionName}-{championDtos.tier}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        } 
    }

    public static string CalculateTeamCompHash(Participant participantDtos)
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
            var cleanedChampionName = ProcessingHelper.CleanChampionName(unit.character_id);
            if (cleanedChampionName == null) continue;
            contentToHash += CalculateWeakChampionHash(cleanedChampionName, unit);
        }
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}