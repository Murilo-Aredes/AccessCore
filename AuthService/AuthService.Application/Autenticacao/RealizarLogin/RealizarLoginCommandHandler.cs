using System.Text.Json;
using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Dispositivos;
using AuthService.Domain.Seguranca;
using AuthService.Domain.Sessoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Autenticacao.RealizarLogin;

public sealed class RealizarLoginCommandHandler(
    IAuthDbContext dbContext,
    IHashSenhaUsuario hashSenhaUsuario,
    ITokenServico tokenServico,
    IProvedorDataHora provedorDataHora,
    IOptions<SegurancaLoginConfiguracao> segurancaLoginConfiguracao,
    IOptions<SessaoConfiguracao> sessaoConfiguracao,
    IOptions<TokenAtualizacaoConfiguracao> tokenAtualizacaoConfiguracao)
    : IManipuladorRequisicao<RealizarLoginCommand, RealizarLoginResponse>
{
    public async Task<RealizarLoginResponse> Handle(RealizarLoginCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var emailNormalizado = requisicao.Email.Trim().ToUpperInvariant();

        var aplicacao = await dbContext.Aplicacoes
            .FirstOrDefaultAsync(x => x.Id == requisicao.AplicacaoId && x.Ativo, cancellationToken);

        if (aplicacao is null)
        {
            throw new NaoAutenticadoException("Aplicação inválida ou inativa.");
        }

        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);

        if (usuario is null)
        {
            await RegistrarFalhaAsync(null, requisicao, "UsuarioNaoEncontrado", agora, cancellationToken);
            throw new NaoAutenticadoException("Credenciais inválidas.");
        }

        if (usuario.Bloqueado && !usuario.EstaBloqueado(agora))
        {
            usuario.Desbloquear(agora);
            dbContext.EventosSeguranca.Add(CriarEvento(usuario.Id, null, aplicacao.Id, requisicao, TipoEventoSeguranca.UsuarioDesbloqueado, NivelEventoSeguranca.Informacao, "Usuário desbloqueado automaticamente.", agora));
        }

        if (!usuario.Ativo)
        {
            await RegistrarFalhaAsync(usuario.Id, requisicao, "UsuarioInativo", agora, cancellationToken);
            throw new ConflitoException("Usuário inativo.");
        }

        if (usuario.EstaBloqueado(agora))
        {
            await RegistrarFalhaAsync(usuario.Id, requisicao, "UsuarioBloqueado", agora, cancellationToken);
            throw new ConflitoException("Usuário temporariamente bloqueado.");
        }

        if (!hashSenhaUsuario.Verificar(usuario, requisicao.Senha))
        {
            var configuracaoBloqueio = segurancaLoginConfiguracao.Value;
            var bloqueou = usuario.RegistrarTentativaLoginInvalida(
                configuracaoBloqueio.QuantidadeMaximaTentativas,
                configuracaoBloqueio.MinutosBloqueio,
                agora);

            dbContext.TentativasLogin.Add(CriarTentativa(usuario.Id, requisicao, false, "SenhaInvalida", agora));
            dbContext.EventosSeguranca.Add(CriarEvento(usuario.Id, null, aplicacao.Id, requisicao, TipoEventoSeguranca.LoginFalhou, NivelEventoSeguranca.Atencao, "Senha inválida informada.", agora));

            if (bloqueou)
            {
                dbContext.EventosSeguranca.Add(CriarEvento(usuario.Id, null, aplicacao.Id, requisicao, TipoEventoSeguranca.UsuarioBloqueado, NivelEventoSeguranca.Critico, "Usuário bloqueado por excesso de tentativas inválidas.", agora));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            throw new NaoAutenticadoException("Credenciais inválidas.");
        }

        usuario.RegistrarTentativaLoginValida(agora);
        dbContext.TentativasLogin.Add(CriarTentativa(usuario.Id, requisicao, true, null, agora));

        var dispositivo = await dbContext.DispositivosConfiaveis
            .FirstOrDefaultAsync(x => x.UsuarioId == usuario.Id && x.IdentificadorDispositivo == requisicao.IdentificadorDispositivo, cancellationToken);

        if (dispositivo is null)
        {
            dispositivo = new DispositivoConfiavel(
                usuario.Id,
                requisicao.IdentificadorDispositivo,
                requisicao.TipoDispositivo,
                requisicao.Navegador,
                requisicao.SistemaOperacional,
                requisicao.TipoDispositivo,
                agora);
            dbContext.DispositivosConfiaveis.Add(dispositivo);
        }

        var dataExpiracaoSessao = agora.AddDays(sessaoConfiguracao.Value.DiasExpiracao);
        var sessao = new SessaoUsuario(
            usuario.Id,
            aplicacao.Id,
            requisicao.IdentificadorDispositivo,
            requisicao.EnderecoIp,
            requisicao.UserAgent,
            requisicao.Navegador,
            requisicao.SistemaOperacional,
            requisicao.TipoDispositivo,
            requisicao.Latitude,
            requisicao.Longitude,
            SituacaoSessao.Pendente,
            agora,
            dataExpiracaoSessao);

        sessao.DefinirLocalizacaoAproximada(
            NormalizarOpcional(requisicao.Cidade),
            NormalizarOpcional(requisicao.Estado),
            NormalizarOpcional(requisicao.Pais));

        dbContext.SessoesUsuarios.Add(sessao);

        if (dispositivo.Confiavel)
        {
            dispositivo.RegistrarAcesso(agora);
            sessao.Confirmar(agora);

            var tokenAcesso = tokenServico.GerarTokenAcesso(usuario, sessao, aplicacao);
            var tokenAtualizacaoValor = tokenServico.GerarTokenSeguro();
            var tokenAtualizacao = new TokenAtualizacao(
                sessao.Id,
                tokenServico.GerarHashToken(tokenAtualizacaoValor),
                agora,
                agora.AddDays(tokenAtualizacaoConfiguracao.Value.DiasExpiracao),
                requisicao.EnderecoIp);

            dbContext.TokensAtualizacao.Add(tokenAtualizacao);
            dbContext.EventosSeguranca.Add(CriarEvento(usuario.Id, sessao.Id, aplicacao.Id, requisicao, TipoEventoSeguranca.LoginRealizado, NivelEventoSeguranca.Informacao, "Login realizado em dispositivo confiável.", agora));

            await dbContext.SaveChangesAsync(cancellationToken);

            return new RealizarLoginResponse(true, false, sessao.Id, tokenAcesso.Valor, tokenAcesso.DataExpiracao, tokenAtualizacaoValor, null, usuario.RequerRedefinicaoSenha);
        }

        var tokenSessaoPendente = tokenServico.GerarTokenSessaoPendente(usuario, sessao, aplicacao);
        dbContext.ConfirmacoesSessao.Add(new ConfirmacaoSessao(
            sessao.Id,
            tokenServico.GerarHashToken(tokenSessaoPendente.Valor),
            agora,
            tokenSessaoPendente.DataExpiracao));

        dbContext.EventosSeguranca.Add(CriarEvento(usuario.Id, sessao.Id, aplicacao.Id, requisicao, TipoEventoSeguranca.NovoDispositivoDetectado, NivelEventoSeguranca.Atencao, "Novo dispositivo detectado. Sessão criada como pendente.", agora));
        dbContext.MensagensOutbox.Add(new MensagemOutbox(
            "NovoLoginDetectado",
            JsonSerializer.Serialize(new
            {
                usuario.Email,
                usuario.Nome,
                SessaoUsuarioId = sessao.Id,
                Dispositivo = requisicao.TipoDispositivo,
                requisicao.Navegador,
                requisicao.SistemaOperacional,
                requisicao.Cidade,
                requisicao.Estado,
                requisicao.Pais,
                requisicao.Latitude,
                requisicao.Longitude,
                EnderecoIpMascarado = MascararEnderecoIp(requisicao.EnderecoIp),
                DataHora = agora,
                TokenConfirmacao = tokenSessaoPendente.Valor
            }),
            agora));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RealizarLoginResponse(false, true, sessao.Id, null, null, null, tokenSessaoPendente.Valor, usuario.RequerRedefinicaoSenha);
    }

    private async Task RegistrarFalhaAsync(Guid? usuarioId, RealizarLoginCommand requisicao, string motivoFalha, DateTimeOffset agora, CancellationToken cancellationToken)
    {
        dbContext.TentativasLogin.Add(CriarTentativa(usuarioId, requisicao, false, motivoFalha, agora));
        dbContext.EventosSeguranca.Add(CriarEvento(usuarioId, null, requisicao.AplicacaoId, requisicao, TipoEventoSeguranca.LoginFalhou, NivelEventoSeguranca.Atencao, motivoFalha, agora));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TentativaLogin CriarTentativa(Guid? usuarioId, RealizarLoginCommand requisicao, bool sucesso, string? motivoFalha, DateTimeOffset agora)
    {
        return new TentativaLogin(usuarioId, requisicao.Email, requisicao.EnderecoIp, requisicao.IdentificadorDispositivo, requisicao.UserAgent, sucesso, motivoFalha, agora);
    }

    private static EventoSeguranca CriarEvento(Guid? usuarioId, Guid? sessaoUsuarioId, Guid? aplicacaoId, RealizarLoginCommand requisicao, TipoEventoSeguranca tipoEvento, NivelEventoSeguranca nivel, string descricao, DateTimeOffset agora)
    {
        return new EventoSeguranca(usuarioId, sessaoUsuarioId, aplicacaoId, tipoEvento, nivel, requisicao.EnderecoIp, requisicao.IdentificadorDispositivo, descricao, null, agora);
    }

    private static string? MascararEnderecoIp(string? enderecoIp)
    {
        if (string.IsNullOrWhiteSpace(enderecoIp))
        {
            return null;
        }

        var partes = enderecoIp.Split('.');
        return partes.Length == 4 ? $"{partes[0]}.{partes[1]}.*.*" : "***";
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
