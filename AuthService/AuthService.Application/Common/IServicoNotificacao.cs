namespace AuthService.Application.Common;

public interface IServicoNotificacao
{
    Task EnviarConfirmacaoEmailAsync(NotificacaoConfirmacaoEmail notificacao, CancellationToken cancellationToken);
    Task EnviarRecuperacaoSenhaAsync(NotificacaoRecuperacaoSenha notificacao, CancellationToken cancellationToken);
    Task EnviarNovoLoginAsync(NotificacaoNovoLogin notificacao, CancellationToken cancellationToken);
}

public sealed record NotificacaoConfirmacaoEmail(
    string Email,
    string NomeUsuario,
    string TokenConfirmacao);

public sealed record NotificacaoRecuperacaoSenha(
    string Email,
    string NomeUsuario,
    string TokenRecuperacao);

public sealed record NotificacaoNovoLogin(
    Guid SessaoUsuarioId,
    string Email,
    string NomeUsuario,
    string? Dispositivo,
    string? Navegador,
    string? SistemaOperacional,
    string? Cidade,
    string? Estado,
    string? Pais,
    decimal? Latitude,
    decimal? Longitude,
    string? LocalizacaoAproximada,
    string? EnderecoIpMascarado,
    DateTimeOffset DataHora,
    string TokenConfirmacao);
