using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Sessoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Autenticacao.AtualizarToken;

public sealed class AtualizarTokenCommandHandler(
    IAuthDbContext dbContext,
    ITokenServico tokenServico,
    IProvedorDataHora provedorDataHora,
    IOptions<TokenAtualizacaoConfiguracao> tokenAtualizacaoConfiguracao)
    : IManipuladorRequisicao<AtualizarTokenCommand, AtualizarTokenResponse>
{
    public async Task<AtualizarTokenResponse> Handle(AtualizarTokenCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var tokenHash = tokenServico.GerarHashToken(requisicao.TokenAtualizacao);

        var tokenAtualizacao = await dbContext.TokensAtualizacao
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (tokenAtualizacao is null)
        {
            throw new NaoAutenticadoException("Token de atualização inválido.");
        }

        var sessao = await dbContext.SessoesUsuarios
            .FirstOrDefaultAsync(x => x.Id == tokenAtualizacao.SessaoUsuarioId, cancellationToken);

        if (sessao is null)
        {
            throw new NaoAutenticadoException("Sessão inválida.");
        }

        if (tokenAtualizacao.DataUtilizacao.HasValue)
        {
            sessao.Revogar("Reutilização de token de atualização detectada.", agora);

            var tokensSessao = await dbContext.TokensAtualizacao
                .Where(x => x.SessaoUsuarioId == sessao.Id)
                .ToListAsync(cancellationToken);

            foreach (var tokenSessao in tokensSessao)
            {
                tokenSessao.Revogar("Reutilização de token de atualização detectada.", agora, requisicao.EnderecoIp);
            }

            dbContext.EventosSeguranca.Add(new EventoSeguranca(
                sessao.UsuarioId,
                sessao.Id,
                sessao.AplicacaoId,
                TipoEventoSeguranca.ReutilizacaoTokenDetectada,
                NivelEventoSeguranca.Critico,
                requisicao.EnderecoIp,
                sessao.IdentificadorDispositivo,
                "Possível roubo de sessão detectado por reutilização de TokenAtualizacao.",
                null,
                agora));

            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ConflitoException("Reutilização de token de atualização detectada. Sessão revogada.");
        }

        if (!tokenAtualizacao.EstaAtivo(agora) || sessao.Situacao != SituacaoSessao.Confirmada || sessao.DataExpiracao <= agora)
        {
            throw new NaoAutenticadoException("Token de atualização expirado, revogado ou associado a uma sessão inválida.");
        }

        var usuario = await dbContext.Usuarios.FirstAsync(x => x.Id == sessao.UsuarioId, cancellationToken);
        var aplicacao = await dbContext.Aplicacoes.FirstAsync(x => x.Id == sessao.AplicacaoId, cancellationToken);

        var novoTokenValor = tokenServico.GerarTokenSeguro();
        var novoToken = new TokenAtualizacao(
            sessao.Id,
            tokenServico.GerarHashToken(novoTokenValor),
            agora,
            agora.AddDays(tokenAtualizacaoConfiguracao.Value.DiasExpiracao),
            requisicao.EnderecoIp);

        dbContext.TokensAtualizacao.Add(novoToken);
        tokenAtualizacao.MarcarComoUtilizado(novoToken.Id, agora);
        sessao.RegistrarAtividade(agora);

        var tokenAcesso = tokenServico.GerarTokenAcesso(usuario, sessao, aplicacao);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AtualizarTokenResponse(tokenAcesso.Valor, tokenAcesso.DataExpiracao, novoTokenValor);
    }
}
