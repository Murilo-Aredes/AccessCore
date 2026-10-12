using AuthService.Application.Auditorias.ObterAuditorias;
using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auditorias.ObterAuditoriaEntidade;

public sealed class ObterAuditoriaEntidadeQueryHandler(IAuthDbContext dbContext)
    : IManipuladorRequisicao<ObterAuditoriaEntidadeQuery, IReadOnlyCollection<AuditoriaResponse>>
{
    public async Task<IReadOnlyCollection<AuditoriaResponse>> Handle(ObterAuditoriaEntidadeQuery requisicao, CancellationToken cancellationToken)
    {
        return await dbContext.Auditorias
            .AsNoTracking()
            .Where(x => x.NomeEntidade == requisicao.Entidade && x.IdentificadorEntidade == requisicao.Id)
            .OrderByDescending(x => x.DataCriacao)
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
