using AuthService.Domain.Common;

namespace AuthService.Domain.Sessoes;

public sealed class TokenAtualizacao : Entidade
{
    private TokenAtualizacao()
    {
    }

    public TokenAtualizacao(Guid sessaoUsuarioId, string tokenHash, DateTimeOffset dataCriacao, DateTimeOffset dataExpiracao, string? enderecoIpCriacao)
    {
        SessaoUsuarioId = sessaoUsuarioId;
        TokenHash = tokenHash;
        DataCriacao = dataCriacao;
        DataExpiracao = dataExpiracao;
        EnderecoIpCriacao = enderecoIpCriacao;
    }

    public Guid SessaoUsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset DataExpiracao { get; private set; }
    public DateTimeOffset? DataUtilizacao { get; private set; }
    public DateTimeOffset? DataRevogacao { get; private set; }
    public Guid? TokenSubstitutoId { get; private set; }
    public string? EnderecoIpCriacao { get; private set; }
    public string? EnderecoIpRevogacao { get; private set; }
    public string? MotivoRevogacao { get; private set; }

    public bool EstaAtivo(DateTimeOffset dataReferencia)
    {
        return DataRevogacao is null && DataUtilizacao is null && DataExpiracao > dataReferencia;
    }

    public void MarcarComoUtilizado(Guid tokenSubstitutoId, DateTimeOffset dataUtilizacao)
    {
        if (DataUtilizacao.HasValue)
        {
            return;
        }

        DataUtilizacao = dataUtilizacao;
        TokenSubstitutoId = tokenSubstitutoId;
    }

    public void Revogar(string motivo, DateTimeOffset dataRevogacao, string? enderecoIpRevogacao)
    {
        if (DataRevogacao.HasValue)
        {
            return;
        }

        DataRevogacao = dataRevogacao;
        EnderecoIpRevogacao = enderecoIpRevogacao;
        MotivoRevogacao = motivo;
    }
}
