using AuthService.Domain.Common;

namespace AuthService.Domain.Escopos;

public sealed class EscopoUsuario : Entidade
{
    private EscopoUsuario()
    {
    }

    public EscopoUsuario(Guid usuarioId, Guid aplicacaoId, TipoEscopo tipoEscopo, string identificadorEscopo, DateTimeOffset dataCriacao)
    {
        UsuarioId = usuarioId;
        AplicacaoId = aplicacaoId;
        TipoEscopo = tipoEscopo;
        IdentificadorEscopo = identificadorEscopo;
        DataCriacao = dataCriacao;
    }

    public Guid UsuarioId { get; private set; }
    public Guid AplicacaoId { get; private set; }
    public TipoEscopo TipoEscopo { get; private set; }
    public string IdentificadorEscopo { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
}
