namespace Builder.Common.Dtos.RiotApi;

public class Unit
{

    public string character_id { get; set; } = "";// Introduced in patch 9.22 with data_version 2.
    public List<string> itemNames { get; set; } = [];// A list of the unit's items.
    public string name { get; set; } = ""; // Unit name, often left blank.
    public int rarity { get; set; } // Unit rarity, does not equate to unit cost.
    public int tier { get; set; } // Unit tier.
}