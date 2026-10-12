using AuthService.Application.Common;
using AuthService.Application.Auditorias.ObterAuditorias;

namespace AuthService.Application.Auditorias.ObterAuditoriaEntidade;

public sealed record ObterAuditoriaEntidadeQuery(string Entidade, string Id) : IRequisicao<IReadOnlyCollection<AuditoriaResponse>>;
