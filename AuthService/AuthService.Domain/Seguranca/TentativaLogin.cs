using AuthService.Domain.Common;

namespace AuthService.Domain.Seguranca;

public sealed class TentativaLogin : Entidade
{
    private TentativaLogin()
    {
    }

    public TentativaLogin(
        Guid? usuarioId,
        string emailInformado,
        string? enderecoIp,
        string identificadorDispositivo,
        string? userAgent,
        bool sucesso,
        string? motivoFalha,
        DateTimeOffset dataTentativa)
    {
        UsuarioId = usuarioId;
        EmailInformado = emailInformado;
        EnderecoIp = enderecoIp;
        IdentificadorDispositivo = identificadorDispositivo;
        UserAgent = userAgent;
        Sucesso = sucesso;
        MotivoFalha = motivoFalha;
        DataTentativa = dataTentativa;
    }

    public Guid? UsuarioId { get; private set; }
    public string EmailInformado { get; private set; } = string.Empty;
    public string? EnderecoIp { get; private set; }
    public string IdentificadorDispositivo { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public bool Sucesso { get; private set; }
    public string? MotivoFalha { get; private set; }
    public DateTimeOffset DataTentativa { get; private set; }
}
