using AuthService.Application.Common;

namespace AuthService.Application.Auditorias.ObterAuditorias;

public sealed record ObterAuditoriasQuery(Guid? UsuarioId, string? NomeEntidade, DateTimeOffset? DataInicio, DateTimeOffset? DataFim) : IRequisicao<IReadOnlyCollection<AuditoriaResponse>>;
