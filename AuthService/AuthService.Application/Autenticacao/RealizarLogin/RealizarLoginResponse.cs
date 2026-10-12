namespace AuthService.Application.Autenticacao.RealizarLogin;

public sealed record RealizarLoginResponse(
    bool Autenticado,
    bool RequerConfirmacaoDispositivo,
    Guid SessaoUsuarioId,
    string? TokenAcesso,
    DateTimeOffset? DataExpiracaoTokenAcesso,
    string? TokenAtualizacao,
    string? TokenSessaoPendente,
    bool RequerRedefinicaoSenha);
