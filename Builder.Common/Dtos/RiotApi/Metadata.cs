namespace Builder.Common.Dtos.RiotApi;
public class Metadata
{
    public string? data_version { get; set; }
    public string? match_id { get; set; }
    public List<string> participants { get; set; } = [];
}