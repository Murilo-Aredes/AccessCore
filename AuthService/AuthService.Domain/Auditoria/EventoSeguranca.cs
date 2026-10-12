using AuthService.Domain.Common;

namespace AuthService.Domain.Auditoria;

public sealed class EventoSeguranca : Entidade
{
    private EventoSeguranca()
    {
    }

    public EventoSeguranca(
        Guid? usuarioId,
        Guid? sessaoUsuarioId,
        Guid? aplicacaoId,
        TipoEventoSeguranca tipoEvento,
        NivelEventoSeguranca nivel,
        string? enderecoIp,
        string? identificadorDispositivo,
        string descricao,
        string? dadosAdicionaisJson,
        DateTimeOffset dataCriacao)
    {
        UsuarioId = usuarioId;
        SessaoUsuarioId = sessaoUsuarioId;
        AplicacaoId = aplicacaoId;
        TipoEvento = tipoEvento;
        Nivel = nivel;
        EnderecoIp = enderecoIp;
        IdentificadorDispositivo = identificadorDispositivo;
        Descricao = descricao;
        DadosAdicionaisJson = dadosAdicionaisJson;
        DataCriacao = dataCriacao;
    }

    public Guid? UsuarioId { get; private set; }
    public Guid? SessaoUsuarioId { get; private set; }
    public Guid? AplicacaoId { get; private set; }
    public TipoEventoSeguranca TipoEvento { get; private set; }
    public NivelEventoSeguranca Nivel { get; private set; }
    public string? EnderecoIp { get; private set; }
    public string? IdentificadorDispositivo { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public string? DadosAdicionaisJson { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
}
