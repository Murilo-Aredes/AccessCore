using AuthService.Application.Common;

namespace AuthService.Application.Sessoes.RejeitarSessao;

public sealed record RejeitarSessaoCommand(Guid SessaoUsuarioId, string TokenSessaoPendente, string? EnderecoIp) : IRequisicao<bool>;
