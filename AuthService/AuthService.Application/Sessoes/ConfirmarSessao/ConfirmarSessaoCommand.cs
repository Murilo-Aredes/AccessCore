using AuthService.Application.Common;

namespace AuthService.Application.Sessoes.ConfirmarSessao;

public sealed record ConfirmarSessaoCommand(
    Guid SessaoUsuarioId,
    string TokenSessaoPendente,
    bool ConfiarDispositivo,
    string? EnderecoIp) : IRequisicao<ConfirmarSessaoResponse>;
