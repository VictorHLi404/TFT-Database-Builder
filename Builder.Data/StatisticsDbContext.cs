using System.ComponentModel.DataAnnotations;
using Builder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Builder.Data;

public class StatisticsDbContext : DbContext
{
    public DbSet<TeamCompEntity> TeamComps { get; set; }
    public DbSet<ChampionEntity> ChampionEntities { get; set; }
    public DbSet<TeamCompChampionJoinEntity> TeamCompChampions { get; set; }

    public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTeamComp(modelBuilder.Entity<TeamCompEntity>());
        ConfigureChampionEntity(modelBuilder.Entity<ChampionEntity>());
        ConfigureTeamCompChampions(modelBuilder.Entity<TeamCompChampionJoinEntity>());
        base.OnModelCreating(modelBuilder);
    }
    private static void ConfigureTeamComp(EntityTypeBuilder<TeamCompEntity> entity)
    {
        entity.HasIndex(t => t.ContentHash)
            .IsUnique();
        entity.Property(t => t.ChampionHashes)
        .HasColumnType("jsonb");
    }

    private static void ConfigureChampionEntity(EntityTypeBuilder<ChampionEntity> entity)
    {
        entity.Property(t => t.Champion)
            .HasConversion<string>();
        entity.HasIndex(t => t.ContentHash)
            .IsUnique();
    }
    private static void ConfigureTeamCompChampions(EntityTypeBuilder<TeamCompChampionJoinEntity> entity)
    {
        entity.HasKey(t => new { t.TeamCompId, t.ChampionEntityId });
        entity.HasOne(tcc => tcc.TeamComp)              // A JoinEntity record has one TeamComp
            .WithMany(tc => tc.Team)     // A TeamComp has many JoinEntity records
            .HasForeignKey(tcc => tcc.TeamCompId);    // The foreign key is TeamCompId
        entity.HasOne(tcc => tcc.Champion)          // A JoinEntity record has one ChampionData
            .WithMany(cd => cd.Team)     // A ChampionData has many JoinEntity records
            .HasForeignKey(tcc => tcc.ChampionEntityId); // The foreign key is Champion
    }


}