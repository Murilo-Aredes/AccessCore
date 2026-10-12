using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Dispositivos;
using AuthService.Domain.Sessoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Sessoes.ConfirmarSessao;

public sealed class ConfirmarSessaoCommandHandler(
    IAuthDbContext dbContext,
    ITokenServico tokenServico,
    IProvedorDataHora provedorDataHora,
    IOptions<TokenAtualizacaoConfiguracao> tokenAtualizacaoConfiguracao)
    : IManipuladorRequisicao<ConfirmarSessaoCommand, ConfirmarSessaoResponse>
{
    public async Task<ConfirmarSessaoResponse> Handle(ConfirmarSessaoCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var tokenHash = tokenServico.GerarHashToken(requisicao.TokenSessaoPendente);

        var confirmacao = await dbContext.ConfirmacoesSessao
            .FirstOrDefaultAsync(x => x.SessaoUsuarioId == requisicao.SessaoUsuarioId && x.TokenHash == tokenHash, cancellationToken);

        if (confirmacao is null || !confirmacao.EstaValida(agora))
        {
            throw new NaoAutenticadoException("Token de sessão pendente inválido ou expirado.");
        }

        var sessao = await dbContext.SessoesUsuarios
            .FirstOrDefaultAsync(x => x.Id == requisicao.SessaoUsuarioId, cancellationToken);

        if (sessao is null)
        {
            throw new NaoEncontradoException("Sessão não encontrada.");
        }

        if (sessao.Situacao != SituacaoSessao.Pendente)
        {
            throw new ConflitoException("Somente sessões pendentes podem ser confirmadas.");
        }

        sessao.Confirmar(agora);
        confirmacao.MarcarComoUtilizada(agora);

        if (requisicao.ConfiarDispositivo)
        {
            var dispositivo = await dbContext.DispositivosConfiaveis
                .FirstOrDefaultAsync(x => x.UsuarioId == sessao.UsuarioId && x.IdentificadorDispositivo == sessao.IdentificadorDispositivo, cancellationToken);

            if (dispositivo is null)
            {
                dispositivo = new DispositivoConfiavel(
                    sessao.UsuarioId,
                    sessao.IdentificadorDispositivo,
                    sessao.TipoDispositivo,
                    sessao.Navegador,
                    sessao.SistemaOperacional,
                    sessao.TipoDispositivo,
                    agora);
                dbContext.DispositivosConfiaveis.Add(dispositivo);
            }

            dispositivo.MarcarComoConfiavel(agora);
        }

        var usuario = await dbContext.Usuarios.FirstAsync(x => x.Id == sessao.UsuarioId, cancellationToken);
        var aplicacao = await dbContext.Aplicacoes.FirstAsync(x => x.Id == sessao.AplicacaoId, cancellationToken);
        var tokenAcesso = tokenServico.GerarTokenAcesso(usuario, sessao, aplicacao);
        var tokenAtualizacaoValor = tokenServico.GerarTokenSeguro();
        var tokenAtualizacao = new TokenAtualizacao(
            sessao.Id,
            tokenServico.GerarHashToken(tokenAtualizacaoValor),
            agora,
            agora.AddDays(tokenAtualizacaoConfiguracao.Value.DiasExpiracao),
            requisicao.EnderecoIp);

        dbContext.TokensAtualizacao.Add(tokenAtualizacao);
        dbContext.EventosSeguranca.Add(new EventoSeguranca(
            sessao.UsuarioId,
            sessao.Id,
            sessao.AplicacaoId,
            TipoEventoSeguranca.SessaoConfirmada,
            NivelEventoSeguranca.Informacao,
            requisicao.EnderecoIp,
            sessao.IdentificadorDispositivo,
            "Sessão pendente confirmada pelo usuário.",
            null,
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ConfirmarSessaoResponse(tokenAcesso.Valor, tokenAcesso.DataExpiracao, tokenAtualizacaoValor, usuario.RequerRedefinicaoSenha);
    }
}
