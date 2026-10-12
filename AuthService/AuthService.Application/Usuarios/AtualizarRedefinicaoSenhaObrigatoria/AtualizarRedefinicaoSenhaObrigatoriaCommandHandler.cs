using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;

public sealed class AtualizarRedefinicaoSenhaObrigatoriaCommandHandler(
    IAuthDbContext dbContext,
    IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<AtualizarRedefinicaoSenhaObrigatoriaCommand, AtualizarRedefinicaoSenhaObrigatoriaResponse>
{
    public async Task<AtualizarRedefinicaoSenhaObrigatoriaResponse> Handle(AtualizarRedefinicaoSenhaObrigatoriaCommand requisicao, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(x => x.Id == requisicao.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException("Usuário não encontrado.");

        var agora = provedorDataHora.UtcNow;
        if (requisicao.RequerRedefinicaoSenha)
        {
            usuario.MarcarRedefinicaoSenhaObrigatoria(agora);
        }
        else
        {
            usuario.RemoverRedefinicaoSenhaObrigatoria(agora);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AtualizarRedefinicaoSenhaObrigatoriaResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.RequerRedefinicaoSenha);
    }
}
