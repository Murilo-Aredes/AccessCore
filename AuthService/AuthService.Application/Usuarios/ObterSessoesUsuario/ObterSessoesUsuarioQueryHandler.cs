using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Usuarios.ObterSessoesUsuario;

public sealed class ObterSessoesUsuarioQueryHandler(IAuthDbContext dbContext)
    : IManipuladorRequisicao<ObterSessoesUsuarioQuery, IReadOnlyCollection<SessaoUsuarioResponse>>
{
    public async Task<IReadOnlyCollection<SessaoUsuarioResponse>> Handle(ObterSessoesUsuarioQuery requisicao, CancellationToken cancellationToken)
    {
        return await dbContext.SessoesUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == requisicao.UsuarioId)
            .OrderByDescending(x => x.DataUltimaAtividade ?? x.DataCriacao)
            .Select(x => new SessaoUsuarioResponse(
                x.Id,
                x.AplicacaoId,
                x.IdentificadorDispositivo,
                x.EnderecoIp,
                x.Navegador,
                x.SistemaOperacional,
                x.TipoDispositivo,
                x.Situacao.ToString(),
                x.DataCriacao,
                x.DataUltimaAtividade,
                x.DataExpiracao))
            .ToListAsync(cancellationToken);
    }
}
