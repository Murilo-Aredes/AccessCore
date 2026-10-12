using AuthService.Domain.Common;

namespace AuthService.Domain.Seguranca;

public sealed class TokenRecuperacaoSenha : Entidade
{
    private TokenRecuperacaoSenha()
    {
    }

    public TokenRecuperacaoSenha(Guid usuarioId, string tokenHash, DateTimeOffset dataCriacao, DateTimeOffset dataExpiracao)
    {
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        DataCriacao = dataCriacao;
        DataExpiracao = dataExpiracao;
    }

    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset DataExpiracao { get; private set; }
    public DateTimeOffset? DataUtilizacao { get; private set; }

    public bool EstaValido(DateTimeOffset dataReferencia) => DataUtilizacao is null && DataExpiracao > dataReferencia;

    public void MarcarComoUtilizado(DateTimeOffset dataUtilizacao) => DataUtilizacao = dataUtilizacao;
}
