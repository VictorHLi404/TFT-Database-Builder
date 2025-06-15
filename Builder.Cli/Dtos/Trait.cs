namespace Builder.Cli.Dtos;
public class Trait
{
    public string name { get; set; } = ""; // Trait name.
    public int num_units { get; set; } // Number of units with this trait.
    public int style { get; set; } // Current style for this trait.
    public int tier_current { get; set; } // Current active tier for the trait.
    public int tier_total { get; set; } // Total tiers for the trait.
}