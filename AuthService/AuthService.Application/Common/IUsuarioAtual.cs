namespace AuthService.Application.Common;

public interface IUsuarioAtual
{
    Guid? UsuarioId { get; }
    Guid? SessaoUsuarioId { get; }
    Guid? AplicacaoId { get; }
    string? EnderecoIp { get; }
    string? IdentificadorDispositivo { get; }
    string IdentificadorCorrelacao { get; }
}
