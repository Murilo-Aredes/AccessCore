using AuthService.Domain.Common;

namespace AuthService.Domain.Sessoes;

public sealed class SessaoUsuario : Entidade
{
    private SessaoUsuario()
    {
    }

    public SessaoUsuario(
        Guid usuarioId,
        Guid aplicacaoId,
        string identificadorDispositivo,
        string? enderecoIp,
        string? userAgent,
        string? navegador,
        string? sistemaOperacional,
        string? tipoDispositivo,
        decimal? latitude,
        decimal? longitude,
        SituacaoSessao situacao,
        DateTimeOffset dataCriacao,
        DateTimeOffset dataExpiracao)
    {
        UsuarioId = usuarioId;
        AplicacaoId = aplicacaoId;
        IdentificadorDispositivo = identificadorDispositivo;
        EnderecoIp = enderecoIp;
        UserAgent = userAgent;
        Navegador = navegador;
        SistemaOperacional = sistemaOperacional;
        TipoDispositivo = tipoDispositivo;
        Latitude = latitude;
        Longitude = longitude;
        Situacao = situacao;
        DataCriacao = dataCriacao;
        DataUltimaAtividade = dataCriacao;
        DataExpiracao = dataExpiracao;
    }

    public Guid UsuarioId { get; private set; }
    public Guid AplicacaoId { get; private set; }
    public string IdentificadorDispositivo { get; private set; } = string.Empty;
    public string? EnderecoIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Navegador { get; private set; }
    public string? SistemaOperacional { get; private set; }
    public string? TipoDispositivo { get; private set; }
    public string? Cidade { get; private set; }
    public string? Estado { get; private set; }
    public string? Pais { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public SituacaoSessao Situacao { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataUltimaAtividade { get; private set; }
    public DateTimeOffset DataExpiracao { get; private set; }
    public DateTimeOffset? DataConfirmacao { get; private set; }
    public DateTimeOffset? DataRevogacao { get; private set; }
    public string? MotivoRevogacao { get; private set; }

    public void DefinirLocalizacaoAproximada(string? cidade, string? estado, string? pais)
    {
        Cidade = cidade;
        Estado = estado;
        Pais = pais;
    }

    public void Confirmar(DateTimeOffset dataConfirmacao)
    {
        if (Situacao == SituacaoSessao.Revogada)
        {
            throw new InvalidOperationException("Uma sessão revogada não pode ser confirmada.");
        }

        if (Situacao == SituacaoSessao.Expirada)
        {
            throw new InvalidOperationException("Uma sessão expirada não pode ser confirmada.");
        }

        Situacao = SituacaoSessao.Confirmada;
        DataConfirmacao = dataConfirmacao;
        DataUltimaAtividade = dataConfirmacao;
    }

    public void Revogar(string motivo, DateTimeOffset dataRevogacao)
    {
        if (Situacao == SituacaoSessao.Revogada)
        {
            return;
        }

        Situacao = SituacaoSessao.Revogada;
        DataRevogacao = dataRevogacao;
        MotivoRevogacao = motivo;
    }

    public void Expirar(DateTimeOffset dataExpiracao)
    {
        if (Situacao == SituacaoSessao.Revogada)
        {
            return;
        }

        Situacao = SituacaoSessao.Expirada;
        DataExpiracao = dataExpiracao;
    }

    public void RegistrarAtividade(DateTimeOffset dataAtividade)
    {
        if (Situacao != SituacaoSessao.Confirmada)
        {
            throw new InvalidOperationException("Somente uma sessão confirmada pode registrar atividade.");
        }

        DataUltimaAtividade = dataAtividade;
    }
}
