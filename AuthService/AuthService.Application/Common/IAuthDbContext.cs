using AuthService.Domain.Aplicacoes;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Dispositivos;
using AuthService.Domain.Escopos;
using AuthService.Domain.Perfis;
using AuthService.Domain.Permissoes;
using AuthService.Domain.Seguranca;
using AuthService.Domain.Sessoes;
using AuthService.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Common;

public interface IAuthDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Aplicacao> Aplicacoes { get; }
    DbSet<Perfil> Perfis { get; }
    DbSet<Permissao> Permissoes { get; }
    DbSet<UsuarioPerfil> UsuariosPerfis { get; }
    DbSet<PerfilPermissao> PerfisPermissoes { get; }
    DbSet<UsuarioPermissao> UsuariosPermissoes { get; }
    DbSet<EscopoUsuario> EscoposUsuarios { get; }
    DbSet<SessaoUsuario> SessoesUsuarios { get; }
    DbSet<TokenAtualizacao> TokensAtualizacao { get; }
    DbSet<ConfirmacaoSessao> ConfirmacoesSessao { get; }
    DbSet<DispositivoConfiavel> DispositivosConfiaveis { get; }
    DbSet<TentativaLogin> TentativasLogin { get; }
    DbSet<EventoSeguranca> EventosSeguranca { get; }
    DbSet<Auditoria> Auditorias { get; }
    DbSet<TokenRecuperacaoSenha> TokensRecuperacaoSenha { get; }
    DbSet<TokenConfirmacaoEmail> TokensConfirmacaoEmail { get; }
    DbSet<MensagemOutbox> MensagensOutbox { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
