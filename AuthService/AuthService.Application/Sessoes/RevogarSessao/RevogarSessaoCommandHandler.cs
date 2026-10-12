using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Sessoes.RevogarSessao;

public sealed class RevogarSessaoCommandHandler(IAuthDbContext dbContext, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<RevogarSessaoCommand, bool>
{
    public async Task<bool> Handle(RevogarSessaoCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var sessao = await dbContext.SessoesUsuarios.FirstOrDefaultAsync(x => x.Id == requisicao.SessaoUsuarioId, cancellationToken);
        if (sessao is null)
        {
            throw new NaoEncontradoException("Sessão não encontrada.");
        }

        sessao.Revogar(requisicao.Motivo, agora);

        var tokens = await dbContext.TokensAtualizacao.Where(x => x.SessaoUsuarioId == sessao.Id).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revogar(requisicao.Motivo, agora, requisicao.EnderecoIp);
        }

        dbContext.EventosSeguranca.Add(new EventoSeguranca(
            sessao.UsuarioId,
            sessao.Id,
            sessao.AplicacaoId,
            TipoEventoSeguranca.SessaoRevogada,
            NivelEventoSeguranca.Informacao,
            requisicao.EnderecoIp,
            sessao.IdentificadorDispositivo,
            requisicao.Motivo,
            null,
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
