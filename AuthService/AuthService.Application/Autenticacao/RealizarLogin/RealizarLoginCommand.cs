using AuthService.Application.Common;

namespace AuthService.Application.Autenticacao.RealizarLogin;

public sealed record RealizarLoginCommand(
    string Email,
    string Senha,
    Guid AplicacaoId,
    string IdentificadorDispositivo,
    string? UserAgent,
    string? Navegador,
    string? SistemaOperacional,
    string? TipoDispositivo,
    string? Cidade,
    string? Estado,
    string? Pais,
    decimal? Latitude,
    decimal? Longitude,
    string? EnderecoIp) : IRequisicao<RealizarLoginResponse>;
