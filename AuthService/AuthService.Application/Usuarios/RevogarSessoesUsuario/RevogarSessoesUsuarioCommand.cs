using AuthService.Application.Common;

namespace AuthService.Application.Usuarios.RevogarSessoesUsuario;

public sealed record RevogarSessoesUsuarioCommand(Guid UsuarioId, Guid? SessaoAtualId, string Motivo, string? EnderecoIp) : IRequisicao<int>;
