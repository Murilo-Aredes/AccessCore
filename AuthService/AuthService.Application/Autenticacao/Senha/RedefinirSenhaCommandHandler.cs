using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autenticacao.Senha;

public sealed class RedefinirSenhaCommandHandler(
    IAuthDbContext dbContext,
    ITokenServico tokenServico,
    IHashSenhaUsuario hashSenhaUsuario,
    IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<RedefinirSenhaCommand, bool>
{
    public async Task<bool> Handle(RedefinirSenhaCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var emailNormalizado = requisicao.Email.Trim().ToUpperInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (usuario is null)
        {
            throw new NaoAutenticadoException("Token inválido ou expirado.");
        }

        var tokenHash = tokenServico.GerarHashToken(requisicao.Token);
        var token = await dbContext.TokensRecuperacaoSenha
            .FirstOrDefaultAsync(x => x.UsuarioId == usuario.Id && x.TokenHash == tokenHash, cancellationToken);

        if (token is null || !token.EstaValido(agora))
        {
            throw new NaoAutenticadoException("Token inválido ou expirado.");
        }

        usuario.AlterarSenha(hashSenhaUsuario.GerarHash(usuario, requisicao.NovaSenha), agora);
        token.MarcarComoUtilizado(agora);

        dbContext.EventosSeguranca.Add(new EventoSeguranca(
            usuario.Id,
            null,
            null,
            TipoEventoSeguranca.SenhaAlterada,
            NivelEventoSeguranca.Informacao,
            null,
            null,
            "Senha alterada por recuperação de senha.",
            null,
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
