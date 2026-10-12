using AuthService.Application.Common;

namespace AuthService.Application.Usuarios.ObterSessoesUsuario;

public sealed record ObterSessoesUsuarioQuery(Guid UsuarioId) : IRequisicao<IReadOnlyCollection<SessaoUsuarioResponse>>;
