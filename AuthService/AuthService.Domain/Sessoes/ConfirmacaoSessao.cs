using AuthService.Domain.Common;

namespace AuthService.Domain.Sessoes;

public sealed class ConfirmacaoSessao : Entidade
{
    private ConfirmacaoSessao()
    {
    }

    public ConfirmacaoSessao(Guid sessaoUsuarioId, string tokenHash, DateTimeOffset dataCriacao, DateTimeOffset dataExpiracao)
    {
        SessaoUsuarioId = sessaoUsuarioId;
        TokenHash = tokenHash;
        DataCriacao = dataCriacao;
        DataExpiracao = dataExpiracao;
    }

    public Guid SessaoUsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset DataExpiracao { get; private set; }
    public DateTimeOffset? DataUtilizacao { get; private set; }

    public bool EstaValida(DateTimeOffset dataReferencia) => DataUtilizacao is null && DataExpiracao > dataReferencia;

    public void MarcarComoUtilizada(DateTimeOffset dataUtilizacao)
    {
        DataUtilizacao = dataUtilizacao;
    }
}
