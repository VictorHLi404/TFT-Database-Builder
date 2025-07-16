namespace Builder.Common.Dtos.RiotApi;
public class Match
{
    public Metadata metadata { get; set; }
    public Info info { get; set; }

    public Match()
    {
        metadata = new Metadata();
        info = new Info();
    }
}