namespace AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;

public sealed record AtualizarRedefinicaoSenhaObrigatoriaResponse(
    Guid Id,
    string Nome,
    string Email,
    bool RequerRedefinicaoSenha);
