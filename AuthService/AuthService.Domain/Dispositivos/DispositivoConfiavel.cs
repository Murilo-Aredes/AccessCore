using AuthService.Domain.Common;

namespace AuthService.Domain.Dispositivos;

public sealed class DispositivoConfiavel : Entidade
{
    private DispositivoConfiavel()
    {
    }

    public DispositivoConfiavel(
        Guid usuarioId,
        string identificadorDispositivo,
        string? nomeDispositivo,
        string? navegador,
        string? sistemaOperacional,
        string? tipoDispositivo,
        DateTimeOffset dataPrimeiroAcesso)
    {
        UsuarioId = usuarioId;
        IdentificadorDispositivo = identificadorDispositivo;
        NomeDispositivo = nomeDispositivo;
        Navegador = navegador;
        SistemaOperacional = sistemaOperacional;
        TipoDispositivo = tipoDispositivo;
        DataPrimeiroAcesso = dataPrimeiroAcesso;
        DataUltimoAcesso = dataPrimeiroAcesso;
    }

    public Guid UsuarioId { get; private set; }
    public string IdentificadorDispositivo { get; private set; } = string.Empty;
    public string? NomeDispositivo { get; private set; }
    public string? Navegador { get; private set; }
    public string? SistemaOperacional { get; private set; }
    public string? TipoDispositivo { get; private set; }
    public bool Confiavel { get; private set; }
    public DateTimeOffset DataPrimeiroAcesso { get; private set; }
    public DateTimeOffset? DataUltimoAcesso { get; private set; }
    public DateTimeOffset? DataConfianca { get; private set; }
    public DateTimeOffset? DataRevogacao { get; private set; }

    public void RegistrarAcesso(DateTimeOffset dataAcesso)
    {
        DataUltimoAcesso = dataAcesso;
    }

    public void MarcarComoConfiavel(DateTimeOffset dataConfianca)
    {
        Confiavel = true;
        DataConfianca = dataConfianca;
        DataRevogacao = null;
        DataUltimoAcesso = dataConfianca;
    }

    public void RevogarConfianca(DateTimeOffset dataRevogacao)
    {
        Confiavel = false;
        DataRevogacao = dataRevogacao;
    }
}
