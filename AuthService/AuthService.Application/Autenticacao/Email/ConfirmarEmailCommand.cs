using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.Email;

public sealed record ConfirmarEmailCommand(string Email, string Token) : IRequisicao<bool>;
