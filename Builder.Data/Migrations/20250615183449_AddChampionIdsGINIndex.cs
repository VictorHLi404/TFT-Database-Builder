using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Builder.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChampionIdsGINIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE INDEX CONCURRENTLY IF NOT EXISTS ""IX_TeamComps_ChampionIds""
                ON ""TeamComps"" USING GIN (""ChampionIds"")",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP INDEX CONCURRENTLY IF EXISTS ""IX_TeamComps_ChampionIds""",
                suppressTransaction: true);
        }
    }
}
