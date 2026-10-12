using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aplicacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataAtualizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aplicacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessaoUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NomeEntidade = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdentificadorEntidade = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Acao = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DadosAntesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DadosDepoisJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CamposAlteradosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnderecoIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdentificadorCorrelacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmacaoSessao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessaoUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUtilizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmacaoSessao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispositivoConfiavel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeDispositivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Navegador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SistemaOperacional = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TipoDispositivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Confiavel = table.Column<bool>(type: "bit", nullable: false),
                    DataPrimeiroAcesso = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUltimoAcesso = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataConfianca = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataRevogacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivoConfiavel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventoSeguranca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessaoUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EnderecoIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DadosAdicionaisJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventoSeguranca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MensagemOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConteudoJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataProcessamento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErroProcessamento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    QuantidadeTentativas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagemOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfil",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataAtualizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfil", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessaoUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EnderecoIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Navegador = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SistemaOperacional = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TipoDispositivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Situacao = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUltimaAtividade = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataConfirmacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataRevogacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MotivoRevogacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessaoUsuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TentativaLogin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmailInformado = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    EnderecoIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Sucesso = table.Column<bool>(type: "bit", nullable: false),
                    MotivoFalha = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DataTentativa = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TentativaLogin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenAtualizacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessaoUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUtilizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataRevogacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TokenSubstitutoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnderecoIpCriacao = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EnderecoIpRevogacao = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    MotivoRevogacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenAtualizacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenConfirmacaoEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUtilizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenConfirmacaoEmail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenRecuperacaoSenha",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataExpiracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataUtilizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenRecuperacaoSenha", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    EmailNormalizado = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmailConfirmado = table.Column<bool>(type: "bit", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    Bloqueado = table.Column<bool>(type: "bit", nullable: false),
                    QuantidadeTentativasFalhas = table.Column<int>(type: "int", nullable: false),
                    DataFimBloqueio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DataAtualizacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerfilPermissao",
                columns: table => new
                {
                    PerfilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPermissao", x => new { x.PerfilId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_PerfilPermissao_Perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscopoUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AplicacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoEscopo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdentificadorEscopo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscopoUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscopoUsuario_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPerfil",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerfilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPerfil", x => new { x.UsuarioId, x.PerfilId });
                    table.ForeignKey(
                        name: "FK_UsuarioPerfil_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPermissao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoPermissao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPermissao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioPermissao_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Aplicacao",
                columns: new[] { "Id", "Ativo", "Codigo", "DataAtualizacao", "DataCriacao", "Descricao", "Nome" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, "ADMINISTRACAO", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Aplicação administrativa do AuthService.", "Administração" });

            migrationBuilder.InsertData(
                table: "Perfil",
                columns: new[] { "Id", "AplicacaoId", "Ativo", "Codigo", "DataAtualizacao", "DataCriacao", "Descricao", "Nome" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), true, "ADMINISTRADOR", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Perfil com acesso administrativo.", "Administrador" });

            migrationBuilder.InsertData(
                table: "Permissao",
                columns: new[] { "Id", "AplicacaoId", "Codigo", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), new Guid("11111111-1111-1111-1111-111111111111"), "usuarios.visualizar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite visualizar usuários.", "Visualizar usuários" },
                    { new Guid("33333333-3333-3333-3333-333333333302"), new Guid("11111111-1111-1111-1111-111111111111"), "usuarios.criar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite criar usuários.", "Criar usuários" },
                    { new Guid("33333333-3333-3333-3333-333333333303"), new Guid("11111111-1111-1111-1111-111111111111"), "usuarios.editar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite editar usuários.", "Editar usuários" },
                    { new Guid("33333333-3333-3333-3333-333333333304"), new Guid("11111111-1111-1111-1111-111111111111"), "usuarios.excluir", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite excluir usuários.", "Excluir usuários" },
                    { new Guid("33333333-3333-3333-3333-333333333305"), new Guid("11111111-1111-1111-1111-111111111111"), "perfis.visualizar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite visualizar perfis.", "Visualizar perfis" },
                    { new Guid("33333333-3333-3333-3333-333333333306"), new Guid("11111111-1111-1111-1111-111111111111"), "perfis.editar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite editar perfis.", "Editar perfis" },
                    { new Guid("33333333-3333-3333-3333-333333333307"), new Guid("11111111-1111-1111-1111-111111111111"), "auditoria.visualizar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite visualizar auditorias.", "Visualizar auditoria" },
                    { new Guid("33333333-3333-3333-3333-333333333308"), new Guid("11111111-1111-1111-1111-111111111111"), "sessoes.visualizar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite visualizar sessões.", "Visualizar sessões" },
                    { new Guid("33333333-3333-3333-3333-333333333309"), new Guid("11111111-1111-1111-1111-111111111111"), "sessoes.revogar", new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Permite revogar sessões.", "Revogar sessões" }
                });

            migrationBuilder.InsertData(
                table: "PerfilPermissao",
                columns: new[] { "PerfilId", "PermissaoId", "DataCriacao" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333301"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333302"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333303"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333304"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333305"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333306"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333307"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333308"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333309"), new DateTimeOffset(new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aplicacao_Codigo",
                table: "Aplicacao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_DataCriacao",
                table: "Auditoria",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_NomeEntidade_IdentificadorEntidade",
                table: "Auditoria",
                columns: new[] { "NomeEntidade", "IdentificadorEntidade" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_UsuarioId",
                table: "Auditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmacaoSessao_TokenHash",
                table: "ConfirmacaoSessao",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispositivoConfiavel_UsuarioId_IdentificadorDispositivo",
                table: "DispositivoConfiavel",
                columns: new[] { "UsuarioId", "IdentificadorDispositivo" });

            migrationBuilder.CreateIndex(
                name: "IX_EscopoUsuario_UsuarioId_AplicacaoId_TipoEscopo_IdentificadorEscopo",
                table: "EscopoUsuario",
                columns: new[] { "UsuarioId", "AplicacaoId", "TipoEscopo", "IdentificadorEscopo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventoSeguranca_UsuarioId_DataCriacao",
                table: "EventoSeguranca",
                columns: new[] { "UsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_MensagemOutbox_DataProcessamento_DataCriacao",
                table: "MensagemOutbox",
                columns: new[] { "DataProcessamento", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Perfil_AplicacaoId_Codigo",
                table: "Perfil",
                columns: new[] { "AplicacaoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissao_AplicacaoId_Codigo",
                table: "Permissao",
                columns: new[] { "AplicacaoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessaoUsuario_IdentificadorDispositivo",
                table: "SessaoUsuario",
                column: "IdentificadorDispositivo");

            migrationBuilder.CreateIndex(
                name: "IX_SessaoUsuario_Situacao",
                table: "SessaoUsuario",
                column: "Situacao");

            migrationBuilder.CreateIndex(
                name: "IX_SessaoUsuario_UsuarioId",
                table: "SessaoUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TentativaLogin_UsuarioId_DataTentativa",
                table: "TentativaLogin",
                columns: new[] { "UsuarioId", "DataTentativa" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenAtualizacao_SessaoUsuarioId",
                table: "TokenAtualizacao",
                column: "SessaoUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAtualizacao_TokenHash",
                table: "TokenAtualizacao",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenConfirmacaoEmail_TokenHash",
                table: "TokenConfirmacaoEmail",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenRecuperacaoSenha_TokenHash",
                table: "TokenRecuperacaoSenha",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_EmailNormalizado",
                table: "Usuario",
                column: "EmailNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPermissao_UsuarioId_PermissaoId",
                table: "UsuarioPermissao",
                columns: new[] { "UsuarioId", "PermissaoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aplicacao");

            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "ConfirmacaoSessao");

            migrationBuilder.DropTable(
                name: "DispositivoConfiavel");

            migrationBuilder.DropTable(
                name: "EscopoUsuario");

            migrationBuilder.DropTable(
                name: "EventoSeguranca");

            migrationBuilder.DropTable(
                name: "MensagemOutbox");

            migrationBuilder.DropTable(
                name: "PerfilPermissao");

            migrationBuilder.DropTable(
                name: "Permissao");

            migrationBuilder.DropTable(
                name: "SessaoUsuario");

            migrationBuilder.DropTable(
                name: "TentativaLogin");

            migrationBuilder.DropTable(
                name: "TokenAtualizacao");

            migrationBuilder.DropTable(
                name: "TokenConfirmacaoEmail");

            migrationBuilder.DropTable(
                name: "TokenRecuperacaoSenha");

            migrationBuilder.DropTable(
                name: "UsuarioPerfil");

            migrationBuilder.DropTable(
                name: "UsuarioPermissao");

            migrationBuilder.DropTable(
                name: "Perfil");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
