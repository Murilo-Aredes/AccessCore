using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Usuarios.RevogarSessoesUsuario;

public sealed class RevogarSessoesUsuarioCommandHandler(IAuthDbContext dbContext, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<RevogarSessoesUsuarioCommand, int>
{
    public async Task<int> Handle(RevogarSessoesUsuarioCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var sessoes = await dbContext.SessoesUsuarios
            .Where(x => x.UsuarioId == requisicao.UsuarioId && (!requisicao.SessaoAtualId.HasValue || x.Id != requisicao.SessaoAtualId.Value))
            .ToListAsync(cancellationToken);

        foreach (var sessao in sessoes)
        {
            sessao.Revogar(requisicao.Motivo, agora);
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
        }

        var sessaoIds = sessoes.Select(x => x.Id).ToList();
        var tokens = await dbContext.TokensAtualizacao.Where(x => sessaoIds.Contains(x.SessaoUsuarioId)).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revogar(requisicao.Motivo, agora, requisicao.EnderecoIp);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return sessoes.Count;
    }
}
