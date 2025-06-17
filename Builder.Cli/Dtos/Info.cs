namespace Builder.Cli.Dtos;
public class Info
{
    public long game_datetime { get; set; }
    public float game_length { get; set; }

    public string tft_game_type { get; set; } = "";
    public string game_version { get; set; } = "";

    public List<Participant> participants { get; set; } = [];

    public int queue_id { get; set; }

    public int tft_set_number { get; set; }
}