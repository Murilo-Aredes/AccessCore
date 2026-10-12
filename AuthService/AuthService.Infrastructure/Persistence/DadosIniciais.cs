using AuthService.Domain.Aplicacoes;
using AuthService.Domain.Perfis;
using AuthService.Domain.Permissoes;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

internal static class DadosIniciais
{
    public static readonly Guid AplicacaoAdministracaoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PerfilAdministradorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateTimeOffset DataCriacao = new(2026, 10, 10, 0, 0, 0, TimeSpan.Zero);

    private static readonly (Guid Id, string Codigo, string Nome, string Descricao)[] Permissoes =
    [
        (Guid.Parse("33333333-3333-3333-3333-333333333301"), "usuarios.visualizar", "Visualizar usuários", "Permite visualizar usuários."),
        (Guid.Parse("33333333-3333-3333-3333-333333333302"), "usuarios.criar", "Criar usuários", "Permite criar usuários."),
        (Guid.Parse("33333333-3333-3333-3333-333333333303"), "usuarios.editar", "Editar usuários", "Permite editar usuários."),
        (Guid.Parse("33333333-3333-3333-3333-333333333304"), "usuarios.excluir", "Excluir usuários", "Permite excluir usuários."),
        (Guid.Parse("33333333-3333-3333-3333-333333333305"), "perfis.visualizar", "Visualizar perfis", "Permite visualizar perfis."),
        (Guid.Parse("33333333-3333-3333-3333-333333333306"), "perfis.editar", "Editar perfis", "Permite editar perfis."),
        (Guid.Parse("33333333-3333-3333-3333-333333333307"), "auditoria.visualizar", "Visualizar auditoria", "Permite visualizar auditorias."),
        (Guid.Parse("33333333-3333-3333-3333-333333333308"), "sessoes.visualizar", "Visualizar sessões", "Permite visualizar sessões."),
        (Guid.Parse("33333333-3333-3333-3333-333333333309"), "sessoes.revogar", "Revogar sessões", "Permite revogar sessões.")
    ];

    public static void Adicionar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aplicacao>().HasData(new
        {
            Id = AplicacaoAdministracaoId,
            Nome = "Administração",
            Codigo = "ADMINISTRACAO",
            Descricao = "Aplicação administrativa do AuthService.",
            Ativo = true,
            DataCriacao,
            DataAtualizacao = (DateTimeOffset?)DataCriacao
        });

        modelBuilder.Entity<Perfil>().HasData(new
        {
            Id = PerfilAdministradorId,
            AplicacaoId = AplicacaoAdministracaoId,
            Nome = "Administrador",
            Codigo = "ADMINISTRADOR",
            Descricao = "Perfil com acesso administrativo.",
            Ativo = true,
            DataCriacao,
            DataAtualizacao = (DateTimeOffset?)DataCriacao
        });

        modelBuilder.Entity<Permissao>().HasData(Permissoes.Select(permissao => new
        {
            permissao.Id,
            AplicacaoId = AplicacaoAdministracaoId,
            permissao.Codigo,
            permissao.Nome,
            permissao.Descricao,
            DataCriacao
        }));

        modelBuilder.Entity<PerfilPermissao>().HasData(Permissoes.Select(permissao => new
        {
            PerfilId = PerfilAdministradorId,
            PermissaoId = permissao.Id,
            DataCriacao
        }));
    }
}
