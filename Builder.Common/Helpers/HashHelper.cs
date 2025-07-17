using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Builder.Common.Dtos.LambdaApi;
using Builder.Common.Dtos.RiotApi;
using Builder.Common.Models.Hashes;

namespace Builder.Common.Helpers;

public class HashHelper
{
    public static string CalculateChampionHash(ChampionHashCreateModel request)
    {
        string contentToHash = $"{request.ChampionName}-{request.Items}-{request.Level}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string CalculateWeakChampionHash(WeakChampionHashCreateModel request)
    {
        string contentToHash = $"{request.ChampionName}-{request.Level}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string CalculateTeamCompHash(TeamCompHashCreateModel request)
    {
        string contentToHash = "";
        foreach (var championHashRequest in request.ChampionHashRequests)
        {
            contentToHash += CalculateWeakChampionHash(championHashRequest);
        }
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(contentToHash));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}