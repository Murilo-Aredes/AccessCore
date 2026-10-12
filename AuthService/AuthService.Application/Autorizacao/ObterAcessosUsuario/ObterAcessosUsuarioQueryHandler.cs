using AuthService.Application.Common;
using AuthService.Domain.Permissoes;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autorizacao.ObterAcessosUsuario;

public sealed class ObterAcessosUsuarioQueryHandler(IAuthDbContext dbContext)
    : IManipuladorRequisicao<ObterAcessosUsuarioQuery, ObterAcessosUsuarioResponse>
{
    public async Task<ObterAcessosUsuarioResponse> Handle(ObterAcessosUsuarioQuery requisicao, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requisicao.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException("Usuário não encontrado.");

        var aplicacao = await dbContext.Aplicacoes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requisicao.AplicacaoId, cancellationToken)
            ?? throw new NaoEncontradoException("Aplicação não encontrada.");

        var perfilIds = await dbContext.UsuariosPerfis
            .AsNoTracking()
            .Where(x => x.UsuarioId == requisicao.UsuarioId)
            .Select(x => x.PerfilId)
            .ToListAsync(cancellationToken);

        var perfis = await dbContext.Perfis
            .AsNoTracking()
            .Where(x => perfilIds.Contains(x.Id) && x.AplicacaoId == requisicao.AplicacaoId && x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        var perfilIdsAplicacao = perfis.Select(x => x.Id).ToList();

        var permissoesPerfil = await (
            from perfilPermissao in dbContext.PerfisPermissoes.AsNoTracking()
            join permissao in dbContext.Permissoes.AsNoTracking() on perfilPermissao.PermissaoId equals permissao.Id
            where perfilIdsAplicacao.Contains(perfilPermissao.PerfilId) && permissao.AplicacaoId == requisicao.AplicacaoId
            select permissao.Codigo)
            .ToListAsync(cancellationToken);

        var permissoesUsuario = await (
            from usuarioPermissao in dbContext.UsuariosPermissoes.AsNoTracking()
            join permissao in dbContext.Permissoes.AsNoTracking() on usuarioPermissao.PermissaoId equals permissao.Id
            where usuarioPermissao.UsuarioId == requisicao.UsuarioId && permissao.AplicacaoId == requisicao.AplicacaoId
            select new { permissao.Codigo, usuarioPermissao.TipoPermissao })
            .ToListAsync(cancellationToken);

        var permissoesNegadas = permissoesUsuario
            .Where(x => x.TipoPermissao == TipoPermissaoUsuario.Negar)
            .Select(x => x.Codigo)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var permissoesExplicitas = permissoesUsuario
            .Where(x => x.TipoPermissao == TipoPermissaoUsuario.Permitir)
            .Select(x => x.Codigo);

        var permissoes = permissoesPerfil
            .Concat(permissoesExplicitas)
            .Where(x => !permissoesNegadas.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var escopos = await dbContext.EscoposUsuarios
            .AsNoTracking()
            .Where(x => x.UsuarioId == requisicao.UsuarioId && x.AplicacaoId == requisicao.AplicacaoId)
            .OrderBy(x => x.TipoEscopo)
            .ThenBy(x => x.IdentificadorEscopo)
            .Select(x => new EscopoAcessoResponse(x.TipoEscopo.ToString(), x.IdentificadorEscopo))
            .ToListAsync(cancellationToken);

        return new ObterAcessosUsuarioResponse(
            new UsuarioAcessoResponse(usuario.Id, usuario.Nome),
            new AplicacaoAcessoResponse(aplicacao.Id, aplicacao.Codigo, aplicacao.Nome),
            perfis.Select(x => x.Nome).ToList(),
            permissoes,
            escopos);
    }
}
