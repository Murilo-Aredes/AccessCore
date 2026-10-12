using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Usuarios.ObterUsuarioAtual;

public sealed class ObterUsuarioAtualQueryHandler(IAuthDbContext dbContext)
    : IManipuladorRequisicao<ObterUsuarioAtualQuery, UsuarioAtualResponse>
{
    public async Task<UsuarioAtualResponse> Handle(ObterUsuarioAtualQuery requisicao, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == requisicao.UsuarioId, cancellationToken)
            ?? throw new NaoEncontradoException("Usuário não encontrado.");

        return new UsuarioAtualResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.EmailConfirmado, requisicao.SessaoUsuarioId, requisicao.AplicacaoId);
    }
}
