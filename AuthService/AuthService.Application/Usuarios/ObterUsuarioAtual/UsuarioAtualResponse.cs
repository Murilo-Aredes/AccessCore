namespace AuthService.Application.Usuarios.ObterUsuarioAtual;

public sealed record UsuarioAtualResponse(
    Guid UsuarioId,
    string Nome,
    string Email,
    bool EmailConfirmado,
    Guid? SessaoUsuarioId,
    Guid? AplicacaoId);
