using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Builder.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeakChampionEntityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeakChampionEntities",
                columns: table => new
                {
                    WeakChampionEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Champion = table.Column<string>(type: "text", nullable: false),
                    ChampionLevel = table.Column<int>(type: "integer", nullable: false),
                    AveragePlacement = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalInstances = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeakChampionEntities", x => x.WeakChampionEntityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeakChampionEntities_ContentHash",
                table: "WeakChampionEntities",
                column: "ContentHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeakChampionEntities");
        }
    }
}
