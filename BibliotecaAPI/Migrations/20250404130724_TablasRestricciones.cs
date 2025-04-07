using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablasRestricciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestriccionesDominio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LlaveId = table.Column<int>(type: "int", nullable: false),
                    Dominio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LlaveAPIId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestriccionesDominio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestriccionesDominio_LlavesAPI_LlaveAPIId",
                        column: x => x.LlaveAPIId,
                        principalTable: "LlavesAPI",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RestriccionesIP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LlaveId = table.Column<int>(type: "int", nullable: false),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LlaveAPIId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestriccionesIP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestriccionesIP_LlavesAPI_LlaveAPIId",
                        column: x => x.LlaveAPIId,
                        principalTable: "LlavesAPI",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesDominio_LlaveAPIId",
                table: "RestriccionesDominio",
                column: "LlaveAPIId");

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesIP_LlaveAPIId",
                table: "RestriccionesIP",
                column: "LlaveAPIId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestriccionesDominio");

            migrationBuilder.DropTable(
                name: "RestriccionesIP");
        }
    }
}
