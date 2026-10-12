using AuthService.Domain.Common;

namespace AuthService.Domain.Auditoria;

public sealed class Auditoria : Entidade
{
    private Auditoria()
    {
    }

    public Auditoria(
        Guid? usuarioId,
        Guid? sessaoUsuarioId,
        Guid? aplicacaoId,
        string nomeEntidade,
        string identificadorEntidade,
        TipoAcaoAuditoria acao,
        string? dadosAntesJson,
        string? dadosDepoisJson,
        string? camposAlteradosJson,
        string? enderecoIp,
        string? identificadorDispositivo,
        string identificadorCorrelacao,
        DateTimeOffset dataCriacao)
    {
        UsuarioId = usuarioId;
        SessaoUsuarioId = sessaoUsuarioId;
        AplicacaoId = aplicacaoId;
        NomeEntidade = nomeEntidade;
        IdentificadorEntidade = identificadorEntidade;
        Acao = acao;
        DadosAntesJson = dadosAntesJson;
        DadosDepoisJson = dadosDepoisJson;
        CamposAlteradosJson = camposAlteradosJson;
        EnderecoIp = enderecoIp;
        IdentificadorDispositivo = identificadorDispositivo;
        IdentificadorCorrelacao = identificadorCorrelacao;
        DataCriacao = dataCriacao;
    }

    public Guid? UsuarioId { get; private set; }
    public Guid? SessaoUsuarioId { get; private set; }
    public Guid? AplicacaoId { get; private set; }
    public string NomeEntidade { get; private set; } = string.Empty;
    public string IdentificadorEntidade { get; private set; } = string.Empty;
    public TipoAcaoAuditoria Acao { get; private set; }
    public string? DadosAntesJson { get; private set; }
    public string? DadosDepoisJson { get; private set; }
    public string? CamposAlteradosJson { get; private set; }
    public string? EnderecoIp { get; private set; }
    public string? IdentificadorDispositivo { get; private set; }
    public string IdentificadorCorrelacao { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
}
