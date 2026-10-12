using AuthService.Domain.Common;
using AuthService.Domain.Escopos;
using AuthService.Domain.Perfis;
using AuthService.Domain.Permissoes;

namespace AuthService.Domain.Usuarios;

public sealed class Usuario : Entidade
{
    private readonly List<UsuarioPerfil> _perfis = [];
    private readonly List<UsuarioPermissao> _permissoes = [];
    private readonly List<EscopoUsuario> _escopos = [];

    private Usuario()
    {
    }

    public Usuario(string nome, string email, string emailNormalizado, string senhaHash, DateTimeOffset dataCriacao)
    {
        Nome = nome;
        Email = email;
        EmailNormalizado = emailNormalizado;
        SenhaHash = senhaHash;
        Ativo = true;
        DataCriacao = dataCriacao;
        DataAtualizacao = dataCriacao;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string EmailNormalizado { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public bool EmailConfirmado { get; private set; }
    public bool Ativo { get; private set; }
    public bool Bloqueado { get; private set; }
    public bool RequerRedefinicaoSenha { get; private set; }
    public int QuantidadeTentativasFalhas { get; private set; }
    public DateTimeOffset? DataFimBloqueio { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataAtualizacao { get; private set; }
    public IReadOnlyCollection<UsuarioPerfil> Perfis => _perfis;
    public IReadOnlyCollection<UsuarioPermissao> Permissoes => _permissoes;
    public IReadOnlyCollection<EscopoUsuario> Escopos => _escopos;

    public void AlterarSenha(string senhaHash, DateTimeOffset dataAtualizacao)
    {
        SenhaHash = senhaHash;
        QuantidadeTentativasFalhas = 0;
        Bloqueado = false;
        DataFimBloqueio = null;
        RequerRedefinicaoSenha = false;
        DataAtualizacao = dataAtualizacao;
    }

    public void MarcarRedefinicaoSenhaObrigatoria(DateTimeOffset dataAtualizacao)
    {
        RequerRedefinicaoSenha = true;
        DataAtualizacao = dataAtualizacao;
    }

    public void RemoverRedefinicaoSenhaObrigatoria(DateTimeOffset dataAtualizacao)
    {
        RequerRedefinicaoSenha = false;
        DataAtualizacao = dataAtualizacao;
    }

    public void ConfirmarEmail(DateTimeOffset dataAtualizacao)
    {
        EmailConfirmado = true;
        DataAtualizacao = dataAtualizacao;
    }

    public void RegistrarTentativaLoginValida(DateTimeOffset dataAtualizacao)
    {
        QuantidadeTentativasFalhas = 0;
        Bloqueado = false;
        DataFimBloqueio = null;
        DataAtualizacao = dataAtualizacao;
    }

    public bool RegistrarTentativaLoginInvalida(int quantidadeMaximaTentativas, int minutosBloqueio, DateTimeOffset dataAtualizacao)
    {
        QuantidadeTentativasFalhas++;
        DataAtualizacao = dataAtualizacao;

        if (QuantidadeTentativasFalhas < quantidadeMaximaTentativas)
        {
            return false;
        }

        Bloquear(dataAtualizacao.AddMinutes(minutosBloqueio), dataAtualizacao);
        return true;
    }

    public bool EstaBloqueado(DateTimeOffset dataReferencia)
    {
        return Bloqueado && (!DataFimBloqueio.HasValue || DataFimBloqueio > dataReferencia);
    }

    public void Desbloquear(DateTimeOffset dataAtualizacao)
    {
        Bloqueado = false;
        DataFimBloqueio = null;
        QuantidadeTentativasFalhas = 0;
        DataAtualizacao = dataAtualizacao;
    }

    public void Bloquear(DateTimeOffset dataFimBloqueio, DateTimeOffset dataAtualizacao)
    {
        Bloqueado = true;
        DataFimBloqueio = dataFimBloqueio;
        DataAtualizacao = dataAtualizacao;
    }

    public void Ativar(DateTimeOffset dataAtualizacao)
    {
        Ativo = true;
        DataAtualizacao = dataAtualizacao;
    }

    public void Desativar(DateTimeOffset dataAtualizacao)
    {
        Ativo = false;
        DataAtualizacao = dataAtualizacao;
    }
}
