using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Builder.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Builder.Data.Entities;

public class ChampionEntity
{
    [Key]
    public Guid ChampionEntityId { get; set; }

    [Required]
    [StringLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    public ChampionEnum Champion { get; set; }

    public int ChampionLevel { get; set; }

    [StringLength(200)]
    public string? Items { get; set; }

    public decimal AveragePlacement { get; set; }
    public int TotalInstances { get; set; }

    public List<TeamCompChampionJoinEntity> Team { get; set; } = new List<TeamCompChampionJoinEntity>();
}
