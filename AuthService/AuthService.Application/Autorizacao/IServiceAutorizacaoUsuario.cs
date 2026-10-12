namespace AuthService.Application.Autorizacao;

public interface IServicoAutorizacaoUsuario
{
    Task<bool> UsuarioPossuiPermissaoAsync(Guid usuarioId, Guid aplicacaoId, string codigoPermissao, CancellationToken cancellationToken);
}
