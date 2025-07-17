using Builder.Common.Constants;

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

    public static string CleanItemName(string itemName)
    {
        // janky as hell, replace with a regex eventually
        var cleanedItemName = itemName.Replace("TFT_Item_", "");
        // janky as hell, replace with a regex eventually
        for (int i = 1; i <= ConstantValues.TFT_SET_NUMBER; i++)
            cleanedItemName = cleanedItemName.Replace($"TFT{i}_Item_", "");

        cleanedItemName = itemName.Replace("TFT_", "");
        for (int i = 1; i <= ConstantValues.TFT_SET_NUMBER; i++)
            cleanedItemName = cleanedItemName.Replace($"TFT{i}_", "");

        return cleanedItemName;
    }

    public static string GetItemString(List<string> items)
    {
        items.Sort();
        items = items.Select(x => CleanItemName(x)).ToList();
        return string.Join("-", items).Replace(" ", "").Replace("'", "");
    }
}