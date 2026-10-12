namespace AuthService.Application.Sessoes.ConfirmarSessao;

public sealed record ConfirmarSessaoResponse(
    string TokenAcesso,
    DateTimeOffset DataExpiracaoTokenAcesso,
    string TokenAtualizacao,
    bool RequerRedefinicaoSenha);
