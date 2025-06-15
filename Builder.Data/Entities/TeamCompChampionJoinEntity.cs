namespace Builder.Data.Entities;

public class TeamCompChampionJoinEntity
{
    public Guid TeamCompId { get; set; }
    public Guid ChampionEntityId { get; set; }
    public TeamCompEntity TeamComp { get; set; } = null!;      // Required for EF Core to understand the relationship
    public ChampionEntity Champion { get; set; } = null!; // Required for EF Core to understand the relationship

}