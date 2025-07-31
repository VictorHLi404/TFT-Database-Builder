using Builder.Common.Dtos.RiotApi;
using Builder.Common.Enums;

namespace Builder.Common.Helpers;

public class ProcessingHelper
{
    public static bool DoesChampionExist(string championId)
        => DataDragonProcessingHelper.Instance!.ChampionMapping.ContainsKey(championId);

    public static ChampionEnum GetChampionEnum(string championId)
        => DataDragonProcessingHelper.Instance!.ChampionMapping.ContainsKey(championId) ?
        DataDragonProcessingHelper.Instance!.ChampionMapping[championId] :
        throw new Exception("Cannot get a champion enum of a champion that does not exist.");

    public static bool DoesItemExist(string itemId)
        => DataDragonProcessingHelper.Instance!.ItemMapping.ContainsKey(itemId);

    public static ItemEnum GetItemEnum(string itemId)
        => DataDragonProcessingHelper.Instance!.ItemMapping.ContainsKey(itemId) ?
        DataDragonProcessingHelper.Instance!.ItemMapping[itemId] :
        throw new Exception("Cannot get a item enum of an item that does not exist.");

    public static string? CleanChampionName(string championName)
        => championName.Replace(" ", "").Replace("'", "").Replace(".", "");

    public static string? CleanItemName(string itemName)
        => itemName.Replace(" ", "").Replace("'", "").Replace(".", "");

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

    public static string GetItemString(List<string> items)
    {
        if (items.Any(x => !DoesItemExist(x)))
            return "";
        var cleanedItems = items.Select(x => GetItemEnum(x).ToString()).ToList();
        cleanedItems.Sort();
        return string.Join("-", cleanedItems).Replace(" ", "").Replace("'", "");
    }

    // public static string GetItemString(List<string> items, int currentSetNumber)
    // {
    //     items.Sort();
    //     items = items.Select(x => CleanItemName(x, currentSetNumber)).ToList();
    //     return string.Join("-", items).Replace(" ", "").Replace("'", "");
    // }

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
        var names = units.Select(x => DoesChampionExist(x.character_id) ?
            $"{GetChampionEnum(x.character_id)}{x.tier}"
            : null)
            .Where(x => x != null)
            .ToList()!;

        names.Sort();

        return string.Join("-", names);
    }
}