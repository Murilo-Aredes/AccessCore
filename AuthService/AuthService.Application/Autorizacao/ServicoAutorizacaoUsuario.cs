using AuthService.Application.Common;
using AuthService.Domain.Permissoes;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autorizacao;

public sealed class ServicoAutorizacaoUsuario(IAuthDbContext dbContext) : IServicoAutorizacaoUsuario
{
    public async Task<bool> UsuarioPossuiPermissaoAsync(Guid usuarioId, Guid aplicacaoId, string codigoPermissao, CancellationToken cancellationToken)
    {
        var permissao = await dbContext.Permissoes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AplicacaoId == aplicacaoId && x.Codigo == codigoPermissao, cancellationToken);

        if (permissao is null)
        {
            return false;
        }

        var permissaoUsuario = await dbContext.UsuariosPermissoes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PermissaoId == permissao.Id, cancellationToken);

        if (permissaoUsuario?.TipoPermissao == TipoPermissaoUsuario.Negar)
        {
            return false;
        }

        if (permissaoUsuario?.TipoPermissao == TipoPermissaoUsuario.Permitir)
        {
            return true;
        }

        var perfilIds = await dbContext.UsuariosPerfis
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.PerfilId)
            .ToListAsync(cancellationToken);

        return await dbContext.PerfisPermissoes
            .AsNoTracking()
            .AnyAsync(x => perfilIds.Contains(x.PerfilId) && x.PermissaoId == permissao.Id, cancellationToken);
    }
}
