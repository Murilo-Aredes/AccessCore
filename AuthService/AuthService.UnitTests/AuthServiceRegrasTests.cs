using AuthService.Application.Aplicacoes.CadastrarAplicacao;
using AuthService.Application.Autenticacao.AtualizarToken;
using AuthService.Application.Autenticacao.RealizarLogin;
using AuthService.Application.Autorizacao;
using AuthService.Application.Common;
using AuthService.Application.Sessoes.ConfirmarSessao;
using AuthService.Application.Sessoes.RejeitarSessao;
using AuthService.Application.Sessoes.RevogarSessao;
using AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;
using AuthService.Application.Usuarios.CadastrarUsuario;
using AuthService.Domain.Aplicacoes;
using AuthService.Domain.Dispositivos;
using AuthService.Domain.Perfis;
using AuthService.Domain.Permissoes;
using AuthService.Domain.Sessoes;
using AuthService.Domain.Usuarios;
using AuthService.Infrastructure.Auditoria;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.UnitTests;

public sealed class AuthServiceRegrasTests
{
    private static readonly Guid AplicacaoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PerfilId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private readonly DateTimeOffset _agora = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Login_valido_em_dispositivo_confiavel_gera_tokens()
    {
        var contexto = CriarContexto();
        var usuario = CriarUsuarioComSenha("senha-correta");
        contexto.Add(new Aplicacao("Sistema Empresa", "EMPRESA", null, _agora) { });
        contexto.Entry(contexto.Aplicacoes.Local.Single()).Property(x => x.Id).CurrentValue = AplicacaoId;
        var dispositivo = new DispositivoConfiavel(usuario.Id, "device-1", "Notebook", "Edge", "Windows", "Desktop", _agora);
        dispositivo.MarcarComoConfiavel(_agora);
        contexto.Usuarios.Add(usuario);
        contexto.DispositivosConfiaveis.Add(dispositivo);
        await contexto.SaveChangesAsync();

        var resposta = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        Assert.True(resposta.Autenticado);
        Assert.False(resposta.RequerConfirmacaoDispositivo);
        Assert.NotNull(resposta.TokenAcesso);
        Assert.NotNull(resposta.TokenAtualizacao);
        Assert.Equal(SituacaoSessao.Confirmada, contexto.SessoesUsuarios.Single().Situacao);
    }

    [Fact]
    public async Task Cadastrar_usuario_cria_senha_hash_e_token_confirmacao()
    {
        var contexto = CriarContexto();

        var resposta = await CriarCadastrarUsuarioHandler(contexto).Handle(new CadastrarUsuarioCommand(
            "Murilo",
            "murilo@example.com",
            "senha-segura-123",
            EnviarConfirmacaoEmail: true), CancellationToken.None);

        var usuario = contexto.Usuarios.Single();
        Assert.Equal(usuario.Id, resposta.Id);
        Assert.Equal("MURILO@EXAMPLE.COM", usuario.EmailNormalizado);
        Assert.NotEqual("senha-segura-123", usuario.SenhaHash);
        Assert.True(new HashSenhaUsuario().Verificar(usuario, "senha-segura-123"));
        Assert.False(usuario.RequerRedefinicaoSenha);
        Assert.False(resposta.RequerRedefinicaoSenha);
        Assert.Single(contexto.TokensConfirmacaoEmail);
        Assert.Single(contexto.MensagensOutbox);
    }

    [Fact]
    public async Task Cadastrar_usuario_com_email_duplicado_retorna_conflito()
    {
        var contexto = CriarContexto();
        var handler = CriarCadastrarUsuarioHandler(contexto);
        await handler.Handle(new CadastrarUsuarioCommand("Murilo", "murilo@example.com", "senha-segura-123"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflitoException>(() => handler.Handle(new CadastrarUsuarioCommand(
            "Outro Murilo",
            "MURILO@example.com",
            "outra-senha-123"), CancellationToken.None));
    }

    [Fact]
    public async Task Atualizar_redefinicao_senha_obrigatoria_marca_usuario()
    {
        var contexto = CriarContexto();
        var usuario = CriarUsuarioComSenha("senha-correta");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var resposta = await new AtualizarRedefinicaoSenhaObrigatoriaCommandHandler(
            contexto,
            new ProvedorDataHoraFixo(_agora)).Handle(new AtualizarRedefinicaoSenhaObrigatoriaCommand(usuario.Id, true), CancellationToken.None);

        Assert.True(usuario.RequerRedefinicaoSenha);
        Assert.True(resposta.RequerRedefinicaoSenha);
    }

    [Fact]
    public async Task Login_retorna_redefinicao_senha_obrigatoria_quando_usuario_estiver_marcado()
    {
        var contexto = CriarContexto();
        var usuario = CriarUsuarioComSenha("senha-correta");
        usuario.MarcarRedefinicaoSenhaObrigatoria(_agora);
        contexto.Add(new Aplicacao("Sistema Empresa", "EMPRESA", null, _agora) { });
        contexto.Entry(contexto.Aplicacoes.Local.Single()).Property(x => x.Id).CurrentValue = AplicacaoId;
        var dispositivo = new DispositivoConfiavel(usuario.Id, "device-1", "Notebook", "Edge", "Windows", "Desktop", _agora);
        dispositivo.MarcarComoConfiavel(_agora);
        contexto.Usuarios.Add(usuario);
        contexto.DispositivosConfiaveis.Add(dispositivo);
        await contexto.SaveChangesAsync();

        var resposta = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        Assert.True(resposta.Autenticado);
        Assert.True(resposta.RequerRedefinicaoSenha);
    }

    [Fact]
    public void Alterar_senha_remove_redefinicao_senha_obrigatoria()
    {
        var usuario = CriarUsuarioComSenha("senha-correta");
        usuario.MarcarRedefinicaoSenhaObrigatoria(_agora);

        usuario.AlterarSenha(new HashSenhaUsuario().GerarHash(usuario, "nova-senha-123"), _agora);

        Assert.False(usuario.RequerRedefinicaoSenha);
    }

    [Fact]
    public async Task Cadastrar_aplicacao_cria_aplicacao_ativa_com_codigo_normalizado()
    {
        var contexto = CriarContexto();

        var resposta = await CriarCadastrarAplicacaoHandler(contexto).Handle(new CadastrarAplicacaoCommand(
            "Portal do Cliente",
            " portal-cliente ",
            " Aplicação externa "), CancellationToken.None);

        var aplicacao = contexto.Aplicacoes.Single();
        Assert.Equal(aplicacao.Id, resposta.Id);
        Assert.Equal("Portal do Cliente", aplicacao.Nome);
        Assert.Equal("PORTAL-CLIENTE", aplicacao.Codigo);
        Assert.Equal("Aplicação externa", aplicacao.Descricao);
        Assert.True(aplicacao.Ativo);
    }

    [Fact]
    public async Task Cadastrar_aplicacao_com_codigo_duplicado_retorna_conflito()
    {
        var contexto = CriarContexto();
        var handler = CriarCadastrarAplicacaoHandler(contexto);
        await handler.Handle(new CadastrarAplicacaoCommand("Portal do Cliente", "PORTAL", null), CancellationToken.None);

        await Assert.ThrowsAsync<ConflitoException>(() => handler.Handle(new CadastrarAplicacaoCommand(
            "Outro Portal",
            "portal",
            null), CancellationToken.None));
    }

    [Fact]
    public async Task Senha_invalida_registra_falha()
    {
        var contexto = await CriarContextoComUsuarioAsync();

        await Assert.ThrowsAsync<NaoAutenticadoException>(() => CriarLoginHandler(contexto).Handle(CriarLogin("senha-errada"), CancellationToken.None));

        Assert.False(contexto.TentativasLogin.Single().Sucesso);
        Assert.Equal("SenhaInvalida", contexto.TentativasLogin.Single().MotivoFalha);
    }

    [Fact]
    public async Task Usuario_inexistente_nao_autentica()
    {
        var contexto = CriarContexto();
        await AdicionarAplicacaoAsync(contexto);

        await Assert.ThrowsAsync<NaoAutenticadoException>(() => CriarLoginHandler(contexto).Handle(CriarLogin("qualquer"), CancellationToken.None));

        Assert.Equal("UsuarioNaoEncontrado", contexto.TentativasLogin.Single().MotivoFalha);
    }

    [Fact]
    public async Task Usuario_inativo_nao_autentica()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        contexto.Usuarios.Single().Desativar(_agora);
        await contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflitoException>(() => CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None));
    }

    [Fact]
    public async Task Usuario_bloqueado_nao_autentica()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        contexto.Usuarios.Single().Bloquear(_agora.AddMinutes(15), _agora);
        await contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflitoException>(() => CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None));
    }

    [Fact]
    public async Task Novo_dispositivo_gera_sessao_pendente()
    {
        var contexto = await CriarContextoComUsuarioAsync();

        var resposta = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        Assert.False(resposta.Autenticado);
        Assert.True(resposta.RequerConfirmacaoDispositivo);
        Assert.NotNull(resposta.TokenSessaoPendente);
        Assert.Equal(SituacaoSessao.Pendente, contexto.SessoesUsuarios.Single().Situacao);
    }

    [Fact]
    public async Task Dispositivo_confiavel_gera_sessao_confirmada()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var usuario = contexto.Usuarios.Single();
        var dispositivo = new DispositivoConfiavel(usuario.Id, "device-1", "Notebook", "Edge", "Windows", "Desktop", _agora);
        dispositivo.MarcarComoConfiavel(_agora);
        contexto.DispositivosConfiaveis.Add(dispositivo);
        await contexto.SaveChangesAsync();

        var resposta = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        Assert.True(resposta.Autenticado);
        Assert.Equal(SituacaoSessao.Confirmada, contexto.SessoesUsuarios.OrderBy(x => x.DataCriacao).Last().Situacao);
    }

    [Fact]
    public async Task Confirmar_sessao_pendente_gera_tokens()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var login = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        var resposta = await CriarConfirmarSessaoHandler(contexto).Handle(new ConfirmarSessaoCommand(login.SessaoUsuarioId, login.TokenSessaoPendente!, true, "127.0.0.1"), CancellationToken.None);

        Assert.NotNull(resposta.TokenAcesso);
        Assert.NotNull(resposta.TokenAtualizacao);
        Assert.Equal(SituacaoSessao.Confirmada, contexto.SessoesUsuarios.Single().Situacao);
        Assert.True(contexto.DispositivosConfiaveis.Single().Confiavel);
    }

    [Fact]
    public async Task Rejeitar_sessao_pendente_revoga_sessao()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var login = await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);

        await CriarRejeitarSessaoHandler(contexto).Handle(new RejeitarSessaoCommand(login.SessaoUsuarioId, login.TokenSessaoPendente!, "127.0.0.1"), CancellationToken.None);

        Assert.Equal(SituacaoSessao.Revogada, contexto.SessoesUsuarios.Single().Situacao);
        Assert.Equal("Sessão rejeitada pelo usuário", contexto.SessoesUsuarios.Single().MotivoRevogacao);
    }

    [Fact]
    public async Task Revogar_sessao_confirmada_revoga_tokens()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var login = await CriarLoginConfirmadoAsync(contexto);

        await new RevogarSessaoCommandHandler(contexto, new ProvedorDataHoraFixo(_agora)).Handle(new RevogarSessaoCommand(login.SessaoUsuarioId, "Teste", "127.0.0.1"), CancellationToken.None);

        Assert.Equal(SituacaoSessao.Revogada, contexto.SessoesUsuarios.Single().Situacao);
        Assert.All(contexto.TokensAtualizacao, token => Assert.NotNull(token.DataRevogacao));
    }

    [Fact]
    public async Task Rotacao_de_token_atualizacao_gera_novo_token()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var login = await CriarLoginConfirmadoAsync(contexto);

        var resposta = await CriarAtualizarTokenHandler(contexto).Handle(new AtualizarTokenCommand(login.TokenAtualizacao!, "127.0.0.1"), CancellationToken.None);

        Assert.NotEqual(login.TokenAtualizacao, resposta.TokenAtualizacao);
        Assert.Equal(2, contexto.TokensAtualizacao.Count());
        Assert.NotNull(contexto.TokensAtualizacao.OrderBy(x => x.DataCriacao).First().DataUtilizacao);
    }

    [Fact]
    public async Task Reutilizacao_de_token_atualizacao_revoga_sessao()
    {
        var contexto = await CriarContextoComUsuarioAsync();
        var login = await CriarLoginConfirmadoAsync(contexto);
        await CriarAtualizarTokenHandler(contexto).Handle(new AtualizarTokenCommand(login.TokenAtualizacao!, "127.0.0.1"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflitoException>(() => CriarAtualizarTokenHandler(contexto).Handle(new AtualizarTokenCommand(login.TokenAtualizacao!, "127.0.0.1"), CancellationToken.None));

        Assert.Equal(SituacaoSessao.Revogada, contexto.SessoesUsuarios.Single().Situacao);
    }

    [Fact]
    public async Task Negacao_explicita_de_permissao_tem_prioridade()
    {
        var (contexto, usuario, permissao) = await CriarContextoAutorizacaoAsync();
        contexto.UsuariosPermissoes.Add(new UsuarioPermissao(usuario.Id, permissao.Id, TipoPermissaoUsuario.Negar, _agora));
        await contexto.SaveChangesAsync();

        var permitido = await new ServicoAutorizacaoUsuario(contexto).UsuarioPossuiPermissaoAsync(usuario.Id, AplicacaoId, permissao.Codigo, CancellationToken.None);

        Assert.False(permitido);
    }

    [Fact]
    public async Task Permissao_explicita_do_usuario_permite_acesso()
    {
        var (contexto, usuario, permissao) = await CriarContextoAutorizacaoAsync();
        contexto.UsuariosPermissoes.Add(new UsuarioPermissao(usuario.Id, permissao.Id, TipoPermissaoUsuario.Permitir, _agora));
        await contexto.SaveChangesAsync();

        var permitido = await new ServicoAutorizacaoUsuario(contexto).UsuarioPossuiPermissaoAsync(usuario.Id, AplicacaoId, permissao.Codigo, CancellationToken.None);

        Assert.True(permitido);
    }

    [Fact]
    public async Task Permissao_herdada_do_perfil_permite_acesso()
    {
        var (contexto, usuario, permissao) = await CriarContextoAutorizacaoAsync();
        contexto.UsuariosPerfis.Add(new UsuarioPerfil(usuario.Id, PerfilId, _agora));
        contexto.PerfisPermissoes.Add(new PerfilPermissao(PerfilId, permissao.Id, _agora));
        await contexto.SaveChangesAsync();

        var permitido = await new ServicoAutorizacaoUsuario(contexto).UsuarioPossuiPermissaoAsync(usuario.Id, AplicacaoId, permissao.Codigo, CancellationToken.None);

        Assert.True(permitido);
    }

    [Fact]
    public async Task Negacao_por_padrao_bloqueia_permissao_nao_concedida()
    {
        var (contexto, usuario, permissao) = await CriarContextoAutorizacaoAsync();

        var permitido = await new ServicoAutorizacaoUsuario(contexto).UsuarioPossuiPermissaoAsync(usuario.Id, AplicacaoId, permissao.Codigo, CancellationToken.None);

        Assert.False(permitido);
    }

    [Fact]
    public async Task Auditoria_de_inclusao_e_criada_automaticamente()
    {
        var contexto = CriarContextoComAuditoria();

        contexto.Usuarios.Add(CriarUsuarioComSenha("senha-correta"));
        await contexto.SaveChangesAsync();

        Assert.Contains(contexto.Auditorias, x => x.Acao == Domain.Auditoria.TipoAcaoAuditoria.Inclusao && x.NomeEntidade == nameof(Usuario));
    }

    [Fact]
    public async Task Auditoria_de_alteracao_registra_campos_alterados()
    {
        var contexto = CriarContextoComAuditoria();
        var usuario = CriarUsuarioComSenha("senha-correta");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        usuario.Desativar(_agora);
        await contexto.SaveChangesAsync();

        Assert.Contains(contexto.Auditorias, x => x.Acao == Domain.Auditoria.TipoAcaoAuditoria.Alteracao && x.CamposAlteradosJson!.Contains("Ativo"));
    }

    [Fact]
    public async Task Auditoria_de_exclusao_guarda_estado_anterior()
    {
        var contexto = CriarContextoComAuditoria();
        var aplicacao = new Aplicacao("Financeiro", "FINANCEIRO", null, _agora);
        contexto.Aplicacoes.Add(aplicacao);
        await contexto.SaveChangesAsync();

        contexto.Aplicacoes.Remove(aplicacao);
        await contexto.SaveChangesAsync();

        Assert.Contains(contexto.Auditorias, x => x.Acao == Domain.Auditoria.TipoAcaoAuditoria.Exclusao && x.DadosAntesJson!.Contains("FINANCEIRO"));
    }

    [Fact]
    public async Task SenhaHash_nao_aparece_em_auditoria()
    {
        var contexto = CriarContextoComAuditoria();

        contexto.Usuarios.Add(CriarUsuarioComSenha("senha-correta"));
        await contexto.SaveChangesAsync();

        Assert.DoesNotContain(contexto.Auditorias, x => (x.DadosDepoisJson ?? string.Empty).Contains("SenhaHash"));
    }

    [Fact]
    public async Task TokenHash_nao_aparece_em_auditoria()
    {
        var contexto = CriarContextoComAuditoria();
        var sessao = new SessaoUsuario(Guid.NewGuid(), AplicacaoId, "device-1", null, null, null, null, null, null, null, SituacaoSessao.Pendente, _agora, _agora.AddDays(1));
        contexto.SessoesUsuarios.Add(sessao);
        contexto.TokensAtualizacao.Add(new TokenAtualizacao(sessao.Id, "hash-secreto", _agora, _agora.AddDays(1), null));
        await contexto.SaveChangesAsync();

        Assert.DoesNotContain(contexto.Auditorias, x => (x.DadosDepoisJson ?? string.Empty).Contains("TokenHash"));
    }

    private async Task<AuthDbContext> CriarContextoComUsuarioAsync()
    {
        var contexto = CriarContexto();
        await AdicionarAplicacaoAsync(contexto);
        contexto.Usuarios.Add(CriarUsuarioComSenha("senha-correta"));
        await contexto.SaveChangesAsync();
        return contexto;
    }

    private async Task AdicionarAplicacaoAsync(AuthDbContext contexto)
    {
        var aplicacao = new Aplicacao("Sistema Empresa", "EMPRESA", null, _agora);
        contexto.Aplicacoes.Add(aplicacao);
        contexto.Entry(aplicacao).Property(x => x.Id).CurrentValue = AplicacaoId;
        await contexto.SaveChangesAsync();
    }

    private Usuario CriarUsuarioComSenha(string senha)
    {
        var usuario = new Usuario("Murilo", "murilo@example.com", "MURILO@EXAMPLE.COM", "temporario", _agora);
        var hash = new HashSenhaUsuario().GerarHash(usuario, senha);
        usuario.AlterarSenha(hash, _agora);
        return usuario;
    }

    private RealizarLoginCommand CriarLogin(string senha)
    {
        return new RealizarLoginCommand("murilo@example.com", senha, AplicacaoId, "device-1", "UserAgent", "Edge", "Windows", "Desktop", null, null, null, null, null, "127.0.0.1");
    }

    private RealizarLoginCommandHandler CriarLoginHandler(AuthDbContext contexto)
    {
        return new RealizarLoginCommandHandler(
            contexto,
            new HashSenhaUsuario(),
            CriarTokenServico(),
            new ProvedorDataHoraFixo(_agora),
            Options.Create(new SegurancaLoginConfiguracao()),
            Options.Create(new SessaoConfiguracao()),
            Options.Create(new TokenAtualizacaoConfiguracao()));
    }

    private CadastrarUsuarioCommandHandler CriarCadastrarUsuarioHandler(AuthDbContext contexto)
    {
        return new CadastrarUsuarioCommandHandler(
            contexto,
            new HashSenhaUsuario(),
            CriarTokenServico(),
            new ProvedorDataHoraFixo(_agora));
    }

    private CadastrarAplicacaoCommandHandler CriarCadastrarAplicacaoHandler(AuthDbContext contexto)
    {
        return new CadastrarAplicacaoCommandHandler(contexto, new ProvedorDataHoraFixo(_agora));
    }

    private ConfirmarSessaoCommandHandler CriarConfirmarSessaoHandler(AuthDbContext contexto)
    {
        return new ConfirmarSessaoCommandHandler(contexto, CriarTokenServico(), new ProvedorDataHoraFixo(_agora), Options.Create(new TokenAtualizacaoConfiguracao()));
    }

    private RejeitarSessaoCommandHandler CriarRejeitarSessaoHandler(AuthDbContext contexto)
    {
        return new RejeitarSessaoCommandHandler(contexto, CriarTokenServico(), new ProvedorDataHoraFixo(_agora));
    }

    private AtualizarTokenCommandHandler CriarAtualizarTokenHandler(AuthDbContext contexto)
    {
        return new AtualizarTokenCommandHandler(contexto, CriarTokenServico(), new ProvedorDataHoraFixo(_agora), Options.Create(new TokenAtualizacaoConfiguracao()));
    }

    private async Task<RealizarLoginResponse> CriarLoginConfirmadoAsync(AuthDbContext contexto)
    {
        var usuario = contexto.Usuarios.Single();
        var dispositivo = new DispositivoConfiavel(usuario.Id, "device-1", "Notebook", "Edge", "Windows", "Desktop", _agora);
        dispositivo.MarcarComoConfiavel(_agora);
        contexto.DispositivosConfiaveis.Add(dispositivo);
        await contexto.SaveChangesAsync();
        return await CriarLoginHandler(contexto).Handle(CriarLogin("senha-correta"), CancellationToken.None);
    }

    private async Task<(AuthDbContext Contexto, Usuario Usuario, Permissao Permissao)> CriarContextoAutorizacaoAsync()
    {
        var contexto = CriarContexto();
        var usuario = CriarUsuarioComSenha("senha-correta");
        var aplicacao = new Aplicacao("Administração", "ADMINISTRACAO", null, _agora);
        var perfil = new Perfil(AplicacaoId, "Administrador", "ADMINISTRADOR", null, _agora);
        var permissao = new Permissao(AplicacaoId, "usuarios.visualizar", "Visualizar usuários", null, _agora);

        contexto.Usuarios.Add(usuario);
        contexto.Aplicacoes.Add(aplicacao);
        contexto.Entry(aplicacao).Property(x => x.Id).CurrentValue = AplicacaoId;
        contexto.Perfis.Add(perfil);
        contexto.Entry(perfil).Property(x => x.Id).CurrentValue = PerfilId;
        contexto.Permissoes.Add(permissao);
        await contexto.SaveChangesAsync();

        return (contexto, usuario, permissao);
    }

    private AuthDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options);
    }

    private AuthDbContext CriarContextoComAuditoria()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditoriaSaveChangesInterceptor(
                new UsuarioAtualFalso(),
                new ContextoCorrelacaoFalso(),
                new ProvedorDataHoraFixo(_agora),
                new PropriedadesSensiveisProvider()))
            .Options;

        return new AuthDbContext(options);
    }

    private ITokenServico CriarTokenServico()
    {
        return new TokenServico(Options.Create(new JwtConfiguracao
        {
            ChaveSecreta = "chave-de-testes-com-mais-de-trinta-e-dois-caracteres"
        }), new ProvedorDataHoraFixo(_agora));
    }

    private sealed class ProvedorDataHoraFixo(DateTimeOffset agora) : IProvedorDataHora
    {
        public DateTimeOffset UtcNow => agora;
    }

    private sealed class ContextoCorrelacaoFalso : IContextoCorrelacao
    {
        public string IdentificadorCorrelacao { get; set; } = "correlacao-teste";
    }

    private sealed class UsuarioAtualFalso : IUsuarioAtual
    {
        public Guid? UsuarioId => Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public Guid? SessaoUsuarioId => null;
        public Guid? AplicacaoId => AuthServiceRegrasTests.AplicacaoId;
        public string? EnderecoIp => "127.0.0.1";
        public string? IdentificadorDispositivo => "device-1";
        public string IdentificadorCorrelacao => "correlacao-teste";
    }
}
