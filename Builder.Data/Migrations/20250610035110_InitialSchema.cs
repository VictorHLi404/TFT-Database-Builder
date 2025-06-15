using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Builder.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChampionEntities",
                columns: table => new
                {
                    ChampionEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Champion = table.Column<string>(type: "text", nullable: false),
                    ChampionLevel = table.Column<int>(type: "integer", nullable: false),
                    Items = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AveragePlacement = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalInstances = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionEntities", x => x.ChampionEntityId);
                });

            migrationBuilder.CreateTable(
                name: "TeamComps",
                columns: table => new
                {
                    TeamCompId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AveragePlacement = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalInstances = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamComps", x => x.TeamCompId);
                });

            migrationBuilder.CreateTable(
                name: "TeamCompChampions",
                columns: table => new
                {
                    TeamCompId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionEntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamCompChampions", x => new { x.TeamCompId, x.ChampionEntityId });
                    table.ForeignKey(
                        name: "FK_TeamCompChampions_ChampionEntities_ChampionEntityId",
                        column: x => x.ChampionEntityId,
                        principalTable: "ChampionEntities",
                        principalColumn: "ChampionEntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamCompChampions_TeamComps_TeamCompId",
                        column: x => x.TeamCompId,
                        principalTable: "TeamComps",
                        principalColumn: "TeamCompId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamCompChampions_ChampionEntityId",
                table: "TeamCompChampions",
                column: "ChampionEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamComps_ContentHash",
                table: "TeamComps",
                column: "ContentHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamCompChampions");

            migrationBuilder.DropTable(
                name: "ChampionEntities");

            migrationBuilder.DropTable(
                name: "TeamComps");
        }
    }
}
