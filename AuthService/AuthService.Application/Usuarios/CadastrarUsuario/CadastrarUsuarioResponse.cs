namespace AuthService.Application.Usuarios.CadastrarUsuario;

public sealed record CadastrarUsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    bool EmailConfirmado,
    bool Ativo,
    bool RequerRedefinicaoSenha);
