using System.Text.Json;
using AuthService.Application.Common;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Outbox;

public sealed class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxBackgroundService> logger,
    IOptions<OutboxConfiguracao> outboxConfiguracao) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Outbox não pôde ser processado. Verifique se o banco foi criado, se as migrations foram aplicadas e se a connection string possui permissão de acesso.");
            }

            var intervalo = Math.Max(5, outboxConfiguracao.Value.IntervaloSegundos);
            await Task.Delay(TimeSpan.FromSeconds(intervalo), stoppingToken);
        }
    }

    private async Task ProcessarAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var provedorDataHora = scope.ServiceProvider.GetRequiredService<IProvedorDataHora>();
        var servicoNotificacao = scope.ServiceProvider.GetRequiredService<IServicoNotificacao>();

        var mensagens = await dbContext.MensagensOutbox
            .Where(x => x.DataProcessamento == null && x.QuantidadeTentativas < 5)
            .OrderBy(x => x.DataCriacao)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var mensagem in mensagens)
        {
            try
            {
                logger.LogInformation("Processando mensagem outbox {MensagemOutboxId} do tipo {Tipo}.", mensagem.Id, mensagem.Tipo);
                await ProcessarMensagemAsync(mensagem.Tipo, mensagem.ConteudoJson, servicoNotificacao, cancellationToken);
                mensagem.MarcarComoProcessada(provedorDataHora.UtcNow);
            }
            catch (MensagemOutboxNaoProcessavelException ex)
            {
                mensagem.MarcarComoNaoProcessavel(provedorDataHora.UtcNow, ex.Message);
                logger.LogWarning(ex, "Mensagem outbox {MensagemOutboxId} não pode ser processada e será ignorada.", mensagem.Id);
            }
            catch (Exception ex)
            {
                mensagem.RegistrarFalha(ex.Message);
                logger.LogError(ex, "Erro ao processar mensagem outbox {MensagemOutboxId}.", mensagem.Id);
            }
        }

        if (mensagens.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task ProcessarMensagemAsync(
        string tipo,
        string conteudoJson,
        IServicoNotificacao servicoNotificacao,
        CancellationToken cancellationToken)
    {
        switch (tipo)
        {
            case "ConfirmacaoEmailSolicitada":
            {
                var payload = Deserializar<ConfirmacaoEmailSolicitadaPayload>(conteudoJson);
                await servicoNotificacao.EnviarConfirmacaoEmailAsync(
                    new NotificacaoConfirmacaoEmail(
                        ObterObrigatorio(payload.Email, nameof(payload.Email)),
                        ObterObrigatorio(payload.Nome, nameof(payload.Nome)),
                        ObterObrigatorio(payload.TokenConfirmacao, nameof(payload.TokenConfirmacao))),
                    cancellationToken);
                return;
            }

            case "RecuperacaoSenhaSolicitada":
            {
                var payload = Deserializar<RecuperacaoSenhaSolicitadaPayload>(conteudoJson);
                await servicoNotificacao.EnviarRecuperacaoSenhaAsync(
                    new NotificacaoRecuperacaoSenha(
                        ObterObrigatorio(payload.Email, nameof(payload.Email)),
                        ObterObrigatorio(payload.Nome, nameof(payload.Nome)),
                        ObterObrigatorio(payload.TokenRecuperacao, nameof(payload.TokenRecuperacao))),
                    cancellationToken);
                return;
            }

            case "NovoLoginDetectado":
            {
                var payload = Deserializar<NovoLoginDetectadoPayload>(conteudoJson);
                await servicoNotificacao.EnviarNovoLoginAsync(
                    new NotificacaoNovoLogin(
                        payload.SessaoUsuarioId ?? payload.Id ?? throw new MensagemOutboxNaoProcessavelException("Campo obrigatório ausente na mensagem outbox: SessaoUsuarioId."),
                        ObterObrigatorio(payload.Email, nameof(payload.Email)),
                        ObterObrigatorio(payload.Nome, nameof(payload.Nome)),
                        payload.Dispositivo,
                        payload.Navegador,
                        payload.SistemaOperacional,
                        payload.Cidade,
                        payload.Estado,
                        payload.Pais,
                        payload.Latitude,
                        payload.Longitude,
                        null,
                        payload.EnderecoIpMascarado,
                        payload.DataHora,
                        ObterObrigatorio(payload.TokenConfirmacao, nameof(payload.TokenConfirmacao))),
                    cancellationToken);
                return;
            }
        }

        throw new InvalidOperationException($"Tipo de mensagem outbox não suportado: {tipo}.");
    }

    private static TPayload Deserializar<TPayload>(string conteudoJson)
    {
        return JsonSerializer.Deserialize<TPayload>(conteudoJson)
            ?? throw new InvalidOperationException("Conteúdo da mensagem outbox inválido.");
    }

    private static string ObterObrigatorio(string? valor, string campo)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? throw new MensagemOutboxNaoProcessavelException($"Campo obrigatório ausente na mensagem outbox: {campo}.")
            : valor;
    }

    private sealed record ConfirmacaoEmailSolicitadaPayload(
        string? Email,
        string? Nome,
        string? TokenConfirmacao);

    private sealed record RecuperacaoSenhaSolicitadaPayload(
        string? Email,
        string? Nome,
        string? TokenRecuperacao);

    private sealed record NovoLoginDetectadoPayload(
        Guid? SessaoUsuarioId,
        Guid? Id,
        string? Email,
        string? Nome,
        string? Dispositivo,
        string? Navegador,
        string? SistemaOperacional,
        string? Cidade,
        string? Estado,
        string? Pais,
        decimal? Latitude,
        decimal? Longitude,
        string? EnderecoIpMascarado,
        DateTimeOffset DataHora,
        string? TokenConfirmacao);

    private sealed class MensagemOutboxNaoProcessavelException(string mensagem) : Exception(mensagem);
}
