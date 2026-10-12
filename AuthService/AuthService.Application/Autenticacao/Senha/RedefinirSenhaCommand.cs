using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.Senha;

public sealed record RedefinirSenhaCommand(string Email, string Token, string NovaSenha) : IRequisicao<bool>;
