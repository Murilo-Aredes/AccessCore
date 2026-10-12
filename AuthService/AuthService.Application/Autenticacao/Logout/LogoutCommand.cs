using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.Logout;

public sealed record LogoutCommand(Guid SessaoUsuarioId, string? EnderecoIp) : IRequisicao<bool>;
