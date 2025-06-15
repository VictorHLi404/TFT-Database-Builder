using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Builder.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChampionContentHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "ChampionEntities",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionEntities_ContentHash",
                table: "ChampionEntities",
                column: "ContentHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChampionEntities_ContentHash",
                table: "ChampionEntities");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "ChampionEntities");
        }
    }
}
