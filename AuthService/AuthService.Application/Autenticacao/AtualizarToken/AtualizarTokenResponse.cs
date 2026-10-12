namespace AuthService.Application.Autenticacao.AtualizarToken;

public sealed record AtualizarTokenResponse(
    string TokenAcesso,
    DateTimeOffset DataExpiracaoTokenAcesso,
    string TokenAtualizacao);
