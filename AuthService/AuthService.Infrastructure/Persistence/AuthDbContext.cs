using AuthService.Application.Common;
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
using AuditoriaEntidade = AuthService.Domain.Auditoria.Auditoria;

namespace AuthService.Infrastructure.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options), IAuthDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Aplicacao> Aplicacoes => Set<Aplicacao>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<UsuarioPerfil> UsuariosPerfis => Set<UsuarioPerfil>();
    public DbSet<PerfilPermissao> PerfisPermissoes => Set<PerfilPermissao>();
    public DbSet<UsuarioPermissao> UsuariosPermissoes => Set<UsuarioPermissao>();
    public DbSet<EscopoUsuario> EscoposUsuarios => Set<EscopoUsuario>();
    public DbSet<SessaoUsuario> SessoesUsuarios => Set<SessaoUsuario>();
    public DbSet<TokenAtualizacao> TokensAtualizacao => Set<TokenAtualizacao>();
    public DbSet<ConfirmacaoSessao> ConfirmacoesSessao => Set<ConfirmacaoSessao>();
    public DbSet<DispositivoConfiavel> DispositivosConfiaveis => Set<DispositivoConfiavel>();
    public DbSet<TentativaLogin> TentativasLogin => Set<TentativaLogin>();
    public DbSet<EventoSeguranca> EventosSeguranca => Set<EventoSeguranca>();
    public DbSet<AuditoriaEntidade> Auditorias => Set<AuditoriaEntidade>();
    public DbSet<TokenRecuperacaoSenha> TokensRecuperacaoSenha => Set<TokenRecuperacaoSenha>();
    public DbSet<TokenConfirmacaoEmail> TokensConfirmacaoEmail => Set<TokenConfirmacaoEmail>();
    public DbSet<MensagemOutbox> MensagensOutbox => Set<MensagemOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        DadosIniciais.Adicionar(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}
