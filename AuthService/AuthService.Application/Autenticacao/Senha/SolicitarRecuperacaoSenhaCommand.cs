using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.Senha;

public sealed record SolicitarRecuperacaoSenhaCommand(string Email) : IRequisicao<MensagemGenericaResponse>;
