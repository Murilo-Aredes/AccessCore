using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Sessoes.RejeitarSessao;

public sealed class RejeitarSessaoCommandHandler(IAuthDbContext dbContext, ITokenServico tokenServico, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<RejeitarSessaoCommand, bool>
{
    public async Task<bool> Handle(RejeitarSessaoCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var tokenHash = tokenServico.GerarHashToken(requisicao.TokenSessaoPendente);

        var confirmacao = await dbContext.ConfirmacoesSessao
            .FirstOrDefaultAsync(x => x.SessaoUsuarioId == requisicao.SessaoUsuarioId && x.TokenHash == tokenHash, cancellationToken);

        if (confirmacao is null || !confirmacao.EstaValida(agora))
        {
            throw new NaoAutenticadoException("Token de sessão pendente inválido ou expirado.");
        }

        var sessao = await dbContext.SessoesUsuarios.FirstOrDefaultAsync(x => x.Id == requisicao.SessaoUsuarioId, cancellationToken);
        if (sessao is null)
        {
            throw new NaoEncontradoException("Sessão não encontrada.");
        }

        sessao.Revogar("Sessão rejeitada pelo usuário", agora);
        confirmacao.MarcarComoUtilizada(agora);

        var tokens = await dbContext.TokensAtualizacao.Where(x => x.SessaoUsuarioId == sessao.Id).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revogar("Sessão rejeitada pelo usuário", agora, requisicao.EnderecoIp);
        }

        dbContext.EventosSeguranca.Add(new EventoSeguranca(
            sessao.UsuarioId,
            sessao.Id,
            sessao.AplicacaoId,
            TipoEventoSeguranca.SessaoRejeitada,
            NivelEventoSeguranca.Critico,
            requisicao.EnderecoIp,
            sessao.IdentificadorDispositivo,
            "Usuário informou que não reconhece o acesso.",
            null,
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
