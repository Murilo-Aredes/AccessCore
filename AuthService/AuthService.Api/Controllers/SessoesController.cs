using AuthService.Application.Common;
using AuthService.Application.Sessoes.ConfirmarSessao;
using AuthService.Application.Sessoes.RejeitarSessao;
using AuthService.Application.Sessoes.RevogarSessao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/sessoes")]
public sealed class SessoesController(IExecutorRequisicoes sender) : ControllerBase
{
    [HttpPost("{id:guid}/confirmar")]
    [ProducesResponseType<ConfirmarSessaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirmar(Guid id, ConfirmarSessaoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ConfirmarSessaoCommand(
            id,
            request.TokenSessaoPendente,
            request.ConfiarDispositivo,
            HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("{id:guid}/rejeitar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Rejeitar(Guid id, RejeitarSessaoRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new RejeitarSessaoCommand(id, request.TokenSessaoPendente, HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);
        return Ok(new { sucesso = true });
    }

    [HttpGet("{id:guid}/confirmar-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmarPorLink(
        Guid id,
        [FromQuery] string token,
        [FromQuery] bool confiarDispositivo,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ConfirmarSessaoCommand(
            id,
            token,
            confiarDispositivo,
            HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);

        return Content(CriarPaginaAcao("Sessão confirmada", "Este dispositivo foi confirmado com sucesso."), "text/html; charset=utf-8");
    }

    [HttpGet("{id:guid}/rejeitar-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RejeitarPorLink(Guid id, [FromQuery] string token, CancellationToken cancellationToken)
    {
        await sender.Send(new RejeitarSessaoCommand(id, token, HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);
        return Content(CriarPaginaAcao("Sessão rejeitada", "A tentativa de acesso foi bloqueada."), "text/html; charset=utf-8");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revogar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RevogarSessaoCommand(id, "Sessão revogada por solicitação autenticada.", HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);
        return Ok(new { sucesso = true });
    }

    private static string CriarPaginaAcao(string titulo, string mensagem)
    {
        return $$"""
            <!doctype html>
            <html lang="pt-BR">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>{{titulo}}</title>
                <style>
                    body { font-family: Arial, sans-serif; background: #f5f7fb; color: #172033; margin: 0; padding: 40px 16px; }
                    main { background: #fff; border: 1px solid #e3e8f2; border-radius: 8px; margin: 0 auto; max-width: 520px; padding: 28px; }
                    h1 { font-size: 24px; margin: 0 0 12px; }
                    p { font-size: 16px; line-height: 1.5; margin: 0; }
                </style>
            </head>
            <body>
                <main>
                    <h1>{{titulo}}</h1>
                    <p>{{mensagem}}</p>
                </main>
            </body>
            </html>
            """;
    }
}

public sealed record ConfirmarSessaoRequest(string TokenSessaoPendente, bool ConfiarDispositivo);

public sealed record RejeitarSessaoRequest(string TokenSessaoPendente);
