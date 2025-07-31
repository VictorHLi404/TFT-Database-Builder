using System.ComponentModel;
using System.Text.Json;
using Builder.Common.Dtos.DataDragon;
using Builder.Common.Enums;
using Builder.Common.Helpers;

public class DataDragonProcessingHelper
{
    public static DataDragonProcessingHelper? Instance { get; set; }
    public Dictionary<string, ItemEnum> ItemMapping { get; set; } = new Dictionary<string, ItemEnum>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ChampionEnum> ChampionMapping { get; set; } = new Dictionary<string, ChampionEnum>(StringComparer.OrdinalIgnoreCase);
    public static readonly string FolderName = "DataDragonFiles";
    public static readonly string ChampionMappingPath = "ChampionMapping.json";
    public static readonly string ItemMappingPath = "ItemMapping.json";
    public async static void Initialize(int setNumber)
    {
        if (Instance == null)
        {
            Instance = new DataDragonProcessingHelper();
        }

        string championContent = await File.ReadAllTextAsync(GetFilePath(ChampionMappingPath));
        List<DataDragonChampionInformation>? championInformation = JsonSerializer.Deserialize<List<DataDragonChampionInformation>>(championContent);
        if (championInformation == null)
            throw new Exception("Failed to deserialize the data dragon information for champions properly.");
        string itemContent = await File.ReadAllTextAsync(GetFilePath(ItemMappingPath));
        List<DataDragonItem>? itemInformation = JsonSerializer.Deserialize<List<DataDragonItem>>(itemContent);
        if (itemInformation == null)
            throw new Exception("Failed to deserialize the data dragon information for items properly");

        foreach (var champion in championInformation)
        {
            if (champion.name == null || champion.character_record!.display_name == null)
                throw new Exception("Champion record not processed correctly.");
            if (!champion.name!.Contains(setNumber.ToString()))
                continue;
            var name = ProcessingHelper.CleanChampionName(champion!.character_record.display_name);
            ChampionEnum championEnum;
            if (!Enum.TryParse(name, out championEnum))
                continue;
            Instance.ChampionMapping.Add(champion.name, championEnum);
        }

        foreach (var item in itemInformation)
        {
            if (string.IsNullOrEmpty(item.name) || string.IsNullOrEmpty(item.nameId))
            {
                Console.WriteLine($"Item record not processed correctly. {item.name}, {item.name}");
                continue;
            }
            var name = ProcessingHelper.CleanItemName(item.name);
            ItemEnum itemEnum;
            if (!Enum.TryParse(name, out itemEnum))
                continue;
            Instance.ItemMapping.Add(item.nameId, itemEnum);
        }
        Console.WriteLine(Instance.ChampionMapping);
        Console.WriteLine(Instance.ItemMapping);
    }

    private static string GetFilePath(string filePath)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var path = Path.Combine(baseDirectory, FolderName, filePath);
        if (!File.Exists(path))
            throw new Exception($"The File {path} could not be found");
        return path;
    }
}