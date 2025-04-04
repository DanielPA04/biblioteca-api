using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreaTableLlavesAPIOlvidadoUsuarioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "LlavesAPI",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "LlavesAPI",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
