using AuthService.Application.Common;

namespace AuthService.Application.Usuarios.ObterUsuarioAtual;

public sealed record ObterUsuarioAtualQuery(Guid UsuarioId, Guid? SessaoUsuarioId, Guid? AplicacaoId) : IRequisicao<UsuarioAtualResponse>;
