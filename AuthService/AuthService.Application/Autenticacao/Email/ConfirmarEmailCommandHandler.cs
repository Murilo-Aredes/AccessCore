using AuthService.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autenticacao.Email;

public sealed class ConfirmarEmailCommandHandler(IAuthDbContext dbContext, ITokenServico tokenServico, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<ConfirmarEmailCommand, bool>
{
    public async Task<bool> Handle(ConfirmarEmailCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var emailNormalizado = requisicao.Email.Trim().ToUpperInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (usuario is null)
        {
            throw new NaoAutenticadoException("Token inválido ou expirado.");
        }

        var tokenHash = tokenServico.GerarHashToken(requisicao.Token);
        var token = await dbContext.TokensConfirmacaoEmail
            .FirstOrDefaultAsync(x => x.UsuarioId == usuario.Id && x.TokenHash == tokenHash, cancellationToken);

        if (token is null || !token.EstaValido(agora))
        {
            throw new NaoAutenticadoException("Token inválido ou expirado.");
        }

        usuario.ConfirmarEmail(agora);
        token.MarcarComoUtilizado(agora);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
