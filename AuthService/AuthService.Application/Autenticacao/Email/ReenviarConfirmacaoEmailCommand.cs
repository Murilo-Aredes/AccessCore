using AuthService.Application.Common;
using AuthService.Application.Autenticacao.Senha;

namespace AuthService.Application.Autenticacao.Email;

public sealed record ReenviarConfirmacaoEmailCommand(string Email) : IRequisicao<MensagemGenericaResponse>;
