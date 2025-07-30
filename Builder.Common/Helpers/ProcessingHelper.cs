using Builder.Common.Dtos.RiotApi;

namespace Builder.Common.Helpers;

public class ProcessingHelper
{
    public static string? CleanChampionName(string championName)
    {
        string newName = championName.Replace("TFT14_", "").Replace(" ", "").Replace("'", "");
        if (newName.Contains("Summon"))
        {
            return null;
        }
        else if (newName.Contains("TFTEvent"))
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

    public static string CleanItemName(string itemName, int currentSetNumber) // fix this later
    {
        // janky as hell, replace with a regex eventually
        var cleanedItemName = itemName.Replace("TFT_Item_", "");
        // janky as hell, replace with a regex eventually
        for (int i = 1; i <= currentSetNumber; i++)
            cleanedItemName = cleanedItemName.Replace($"TFT{i}_Item_", "");

        cleanedItemName = cleanedItemName.Replace("TFT_", "");
        for (int i = 1; i <= currentSetNumber; i++)
            cleanedItemName = cleanedItemName.Replace($"TFT{i}_", "");

        return cleanedItemName;
    }

    public static string GetItemString(List<string> items, int currentSetNumber)
    {
        items.Sort();
        items = items.Select(x => CleanItemName(x, currentSetNumber)).ToList();
        return string.Join("-", items).Replace(" ", "").Replace("'", "");
    }

    public static List<ItemEnum> GetItemEnums(string itemString)
    {
        var list = itemString.Split("-").ToList();
        if (list.Count <  1 || string.IsNullOrEmpty(itemString))
            return [];
        try
            {
                return list.Select(x => (ItemEnum)Enum.Parse(typeof(ItemEnum), x)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse {itemString} into a list of ItemEnums: {ex.Message}");
            }
    }

    public static string GetTeamHelperName(List<Unit> units)
    {
        var names = units.Select(x => CleanChampionName(x.character_id) != null ?
            $"{CleanChampionName(x.character_id)}{x.tier}"
            : null)
            .Where(x => x != null)
            .ToList()!;

        names.Sort();

        return string.Join("-", names);
    }
}