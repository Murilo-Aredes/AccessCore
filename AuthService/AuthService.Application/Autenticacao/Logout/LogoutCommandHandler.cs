using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autenticacao.Logout;

public sealed class LogoutCommandHandler(IAuthDbContext dbContext, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var sessao = await dbContext.SessoesUsuarios.FirstOrDefaultAsync(x => x.Id == requisicao.SessaoUsuarioId, cancellationToken);
        if (sessao is null)
        {
            return true;
        }

        sessao.Revogar("Logout solicitado pelo usuário.", agora);

        var tokens = await dbContext.TokensAtualizacao.Where(x => x.SessaoUsuarioId == sessao.Id).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revogar("Logout solicitado pelo usuário.", agora, requisicao.EnderecoIp);
        }

        dbContext.EventosSeguranca.Add(new EventoSeguranca(
            sessao.UsuarioId,
            sessao.Id,
            sessao.AplicacaoId,
            TipoEventoSeguranca.SessaoRevogada,
            NivelEventoSeguranca.Informacao,
            requisicao.EnderecoIp,
            sessao.IdentificadorDispositivo,
            "Logout realizado.",
            null,
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
