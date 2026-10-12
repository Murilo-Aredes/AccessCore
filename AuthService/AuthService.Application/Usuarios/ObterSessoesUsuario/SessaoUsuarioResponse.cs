namespace AuthService.Application.Usuarios.ObterSessoesUsuario;

public sealed record SessaoUsuarioResponse(
    Guid Id,
    Guid AplicacaoId,
    string IdentificadorDispositivo,
    string? EnderecoIp,
    string? Navegador,
    string? SistemaOperacional,
    string? TipoDispositivo,
    string Situacao,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataUltimaAtividade,
    DateTimeOffset DataExpiracao);
