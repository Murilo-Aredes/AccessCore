using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.AtualizarToken;

public sealed record AtualizarTokenCommand(string TokenAtualizacao, string? EnderecoIp) : IRequisicao<AtualizarTokenResponse>;
