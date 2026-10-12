using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auditorias.ObterAuditorias;

public sealed class ObterAuditoriasQueryHandler(IAuthDbContext dbContext)
    : IManipuladorRequisicao<ObterAuditoriasQuery, IReadOnlyCollection<AuditoriaResponse>>
{
    public async Task<IReadOnlyCollection<AuditoriaResponse>> Handle(ObterAuditoriasQuery requisicao, CancellationToken cancellationToken)
    {
        var query = dbContext.Auditorias.AsNoTracking().AsQueryable();

        if (requisicao.UsuarioId.HasValue)
        {
            query = query.Where(x => x.UsuarioId == requisicao.UsuarioId);
        }

        if (!string.IsNullOrWhiteSpace(requisicao.NomeEntidade))
        {
            query = query.Where(x => x.NomeEntidade == requisicao.NomeEntidade);
        }

        if (requisicao.DataInicio.HasValue)
        {
            query = query.Where(x => x.DataCriacao >= requisicao.DataInicio);
        }

        if (requisicao.DataFim.HasValue)
        {
            query = query.Where(x => x.DataCriacao <= requisicao.DataFim);
        }

        return await query
            .OrderByDescending(x => x.DataCriacao)
            .Take(500)
            .Select(x => new AuditoriaResponse(
                x.Id,
                x.UsuarioId,
                x.SessaoUsuarioId,
                x.AplicacaoId,
                x.NomeEntidade,
                x.IdentificadorEntidade,
                x.Acao,
                x.DadosAntesJson,
                x.DadosDepoisJson,
                x.CamposAlteradosJson,
                x.EnderecoIp,
                x.IdentificadorDispositivo,
                x.IdentificadorCorrelacao,
                x.DataCriacao))
            .ToListAsync(cancellationToken);
    }
}
