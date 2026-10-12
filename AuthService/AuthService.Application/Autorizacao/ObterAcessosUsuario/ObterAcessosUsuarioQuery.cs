using AuthService.Application.Common;

namespace AuthService.Application.Autorizacao.ObterAcessosUsuario;

public sealed record ObterAcessosUsuarioQuery(Guid UsuarioId, Guid AplicacaoId) : IRequisicao<ObterAcessosUsuarioResponse>;
