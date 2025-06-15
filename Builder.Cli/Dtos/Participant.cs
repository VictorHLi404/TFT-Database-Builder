namespace Builder.Cli.Dtos;
public class Participant
{
    public Companion? companion { get; set; } // Participant's companion.
    public int gold_left { get; set; } // Gold left after participant was eliminated.
    public int last_round { get; set; } // The round the participant was eliminated in.
    public int level { get; set; } // Participant Little Legend level.
    public int placement { get; set; } // Participant placement upon elimination.
    public int players_eliminated { get; set; } // Number of players the participant eliminated.
    public string? puuid { get; set; } // Participant unique identifier.
    public string? riotIdGameName { get; set; } // Riot ID game name.
    public string? riotIdTagline { get; set; } // Riot ID tagline.
    public float time_eliminated { get; set; } // Number of seconds before participant was eliminated.
    public int total_damage_to_players { get; set; } // Damage dealt to other players.
    public List<Trait> traits { get; set; } = [];// List of traits for the participant's active units.
    public List<Unit> units { get; set; } = [];// List of active units for the participant.
}