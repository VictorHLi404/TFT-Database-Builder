using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Builder.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace Builder.Data.Entities;

public class WeakChampionEntity
{
    [Key]
    public Guid WeakChampionEntityId { get; set; }

    [Required]
    [StringLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    public ChampionEnum Champion { get; set; }

    public int ChampionLevel { get; set; }

    public decimal AveragePlacement { get; set; }

    public int TotalInstances { get; set; }
}
