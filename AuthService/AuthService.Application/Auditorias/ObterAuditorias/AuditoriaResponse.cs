using AuthService.Domain.Auditoria;

namespace AuthService.Application.Auditorias.ObterAuditorias;

public sealed record AuditoriaResponse(
    Guid Id,
    Guid? UsuarioId,
    Guid? SessaoUsuarioId,
    Guid? AplicacaoId,
    string NomeEntidade,
    string IdentificadorEntidade,
    TipoAcaoAuditoria Acao,
    string? DadosAntesJson,
    string? DadosDepoisJson,
    string? CamposAlteradosJson,
    string? EnderecoIp,
    string? IdentificadorDispositivo,
    string IdentificadorCorrelacao,
    DateTimeOffset DataCriacao);
