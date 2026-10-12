using AuthService.Application.Common;

namespace AuthService.Application.Sessoes.RevogarSessao;

public sealed record RevogarSessaoCommand(Guid SessaoUsuarioId, string Motivo, string? EnderecoIp) : IRequisicao<bool>;
