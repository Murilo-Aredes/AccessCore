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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuditoriaEntidade = AuthService.Domain.Auditoria.Auditoria;

namespace AuthService.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EmailNormalizado).HasMaxLength(320).IsRequired();
        builder.Property(x => x.SenhaHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RequerRedefinicaoSenha).HasDefaultValue(false);
        builder.HasIndex(x => x.EmailNormalizado).IsUnique();
    }
}

public sealed class AplicacaoConfiguration : IEntityTypeConfiguration<Aplicacao>
{
    public void Configure(EntityTypeBuilder<Aplicacao> builder)
    {
        builder.ToTable("Aplicacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public sealed class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfil");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.HasIndex(x => new { x.AplicacaoId, x.Codigo }).IsUnique();
    }
}

public sealed class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("Permissao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.HasIndex(x => new { x.AplicacaoId, x.Codigo }).IsUnique();
    }
}

public sealed class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuarioPerfil");
        builder.HasKey(x => new { x.UsuarioId, x.PerfilId });
    }
}

public sealed class PerfilPermissaoConfiguration : IEntityTypeConfiguration<PerfilPermissao>
{
    public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
    {
        builder.ToTable("PerfilPermissao");
        builder.HasKey(x => new { x.PerfilId, x.PermissaoId });
    }
}

public sealed class UsuarioPermissaoConfiguration : IEntityTypeConfiguration<UsuarioPermissao>
{
    public void Configure(EntityTypeBuilder<UsuarioPermissao> builder)
    {
        builder.ToTable("UsuarioPermissao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoPermissao).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.PermissaoId }).IsUnique();
    }
}

public sealed class EscopoUsuarioConfiguration : IEntityTypeConfiguration<EscopoUsuario>
{
    public void Configure(EntityTypeBuilder<EscopoUsuario> builder)
    {
        builder.ToTable("EscopoUsuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoEscopo).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.IdentificadorEscopo).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.AplicacaoId, x.TipoEscopo, x.IdentificadorEscopo }).IsUnique();
    }
}

public sealed class SessaoUsuarioConfiguration : IEntityTypeConfiguration<SessaoUsuario>
{
    public void Configure(EntityTypeBuilder<SessaoUsuario> builder)
    {
        builder.ToTable("SessaoUsuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EnderecoIp).HasMaxLength(80);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.Navegador).HasMaxLength(150);
        builder.Property(x => x.SistemaOperacional).HasMaxLength(150);
        builder.Property(x => x.TipoDispositivo).HasMaxLength(100);
        builder.Property(x => x.Cidade).HasMaxLength(150);
        builder.Property(x => x.Estado).HasMaxLength(150);
        builder.Property(x => x.Pais).HasMaxLength(150);
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
        builder.Property(x => x.Situacao).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.MotivoRevogacao).HasMaxLength(500);
        builder.HasIndex(x => x.UsuarioId);
        builder.HasIndex(x => x.IdentificadorDispositivo);
        builder.HasIndex(x => x.Situacao);
    }
}

public sealed class TokenAtualizacaoConfiguration : IEntityTypeConfiguration<TokenAtualizacao>
{
    public void Configure(EntityTypeBuilder<TokenAtualizacao> builder)
    {
        builder.ToTable("TokenAtualizacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EnderecoIpCriacao).HasMaxLength(80);
        builder.Property(x => x.EnderecoIpRevogacao).HasMaxLength(80);
        builder.Property(x => x.MotivoRevogacao).HasMaxLength(500);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.SessaoUsuarioId);
    }
}

public sealed class ConfirmacaoSessaoConfiguration : IEntityTypeConfiguration<ConfirmacaoSessao>
{
    public void Configure(EntityTypeBuilder<ConfirmacaoSessao> builder)
    {
        builder.ToTable("ConfirmacaoSessao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

public sealed class DispositivoConfiavelConfiguration : IEntityTypeConfiguration<DispositivoConfiavel>
{
    public void Configure(EntityTypeBuilder<DispositivoConfiavel> builder)
    {
        builder.ToTable("DispositivoConfiavel");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NomeDispositivo).HasMaxLength(150);
        builder.Property(x => x.Navegador).HasMaxLength(150);
        builder.Property(x => x.SistemaOperacional).HasMaxLength(150);
        builder.Property(x => x.TipoDispositivo).HasMaxLength(100);
        builder.HasIndex(x => new { x.UsuarioId, x.IdentificadorDispositivo });
    }
}

public sealed class TentativaLoginConfiguration : IEntityTypeConfiguration<TentativaLogin>
{
    public void Configure(EntityTypeBuilder<TentativaLogin> builder)
    {
        builder.ToTable("TentativaLogin");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailInformado).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EnderecoIp).HasMaxLength(80);
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.MotivoFalha).HasMaxLength(120);
        builder.HasIndex(x => new { x.UsuarioId, x.DataTentativa });
    }
}

public sealed class EventoSegurancaConfiguration : IEntityTypeConfiguration<EventoSeguranca>
{
    public void Configure(EntityTypeBuilder<EventoSeguranca> builder)
    {
        builder.ToTable("EventoSeguranca");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoEvento).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(x => x.Nivel).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.EnderecoIp).HasMaxLength(80);
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(200);
        builder.Property(x => x.Descricao).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.DataCriacao });
    }
}

public sealed class AuditoriaConfiguration : IEntityTypeConfiguration<AuditoriaEntidade>
{
    public void Configure(EntityTypeBuilder<AuditoriaEntidade> builder)
    {
        builder.ToTable("Auditoria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NomeEntidade).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IdentificadorEntidade).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Acao).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.EnderecoIp).HasMaxLength(80);
        builder.Property(x => x.IdentificadorDispositivo).HasMaxLength(200);
        builder.Property(x => x.IdentificadorCorrelacao).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.UsuarioId);
        builder.HasIndex(x => new { x.NomeEntidade, x.IdentificadorEntidade });
        builder.HasIndex(x => x.DataCriacao);
    }
}

public sealed class TokenRecuperacaoSenhaConfiguration : IEntityTypeConfiguration<TokenRecuperacaoSenha>
{
    public void Configure(EntityTypeBuilder<TokenRecuperacaoSenha> builder)
    {
        builder.ToTable("TokenRecuperacaoSenha");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

public sealed class TokenConfirmacaoEmailConfiguration : IEntityTypeConfiguration<TokenConfirmacaoEmail>
{
    public void Configure(EntityTypeBuilder<TokenConfirmacaoEmail> builder)
    {
        builder.ToTable("TokenConfirmacaoEmail");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

public sealed class MensagemOutboxConfiguration : IEntityTypeConfiguration<MensagemOutbox>
{
    public void Configure(EntityTypeBuilder<MensagemOutbox> builder)
    {
        builder.ToTable("MensagemOutbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ConteudoJson).IsRequired();
        builder.Property(x => x.ErroProcessamento).HasMaxLength(2000);
        builder.HasIndex(x => new { x.DataProcessamento, x.DataCriacao });
    }
}
