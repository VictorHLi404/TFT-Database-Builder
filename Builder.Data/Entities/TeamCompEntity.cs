using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;

namespace Builder.Data.Entities;

public class TeamCompEntity
{
    [Key]
    public Guid TeamCompId { get; set; }

    [StringLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    public decimal AveragePlacement { get; set; }

    public int TotalInstances { get; set; }

    public List<TeamCompChampionJoinEntity> Team { get; set; } = new List<TeamCompChampionJoinEntity>();

    public List<string>? ChampionHashes { get; set; } = new List<string>();
}
