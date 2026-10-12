using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaRedefinicaoSenhaObrigatoriaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequerRedefinicaoSenha",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequerRedefinicaoSenha",
                table: "Usuario");
        }
    }
}
