using AuthService.Application.Autenticacao.AtualizarToken;
using AuthService.Application.Autenticacao.Email;
using AuthService.Application.Autenticacao.Logout;
using AuthService.Application.Autenticacao.RealizarLogin;
using AuthService.Application.Autenticacao.Senha;
using AuthService.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public sealed class AutenticacaoController(IExecutorRequisicoes sender, IUsuarioAtual usuarioAtual) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType<RealizarLoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new RealizarLoginCommand(
            request.Email,
            request.Senha,
            request.AplicacaoId,
            request.IdentificadorDispositivo,
            Request.Headers.UserAgent.FirstOrDefault(),
            request.Navegador,
            request.SistemaOperacional,
            request.TipoDispositivo,
            request.Cidade,
            request.Estado,
            request.Pais,
            request.Latitude,
            request.Longitude,
            HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("atualizar-token")]
    [ProducesResponseType<AtualizarTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AtualizarToken(AtualizarTokenRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new AtualizarTokenCommand(request.TokenAtualizacao, HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);
        return Ok(resultado);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!usuarioAtual.SessaoUsuarioId.HasValue)
        {
            throw new NaoAutenticadoException("Sessão não identificada.");
        }

        await sender.Send(new LogoutCommand(usuarioAtual.SessaoUsuarioId.Value, usuarioAtual.EnderecoIp), cancellationToken);
        return Ok(new { sucesso = true });
    }

    [HttpPost("senha/esqueci")]
    [ProducesResponseType<MensagemGenericaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new SolicitarRecuperacaoSenhaCommand(request.Email), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("senha/redefinir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new RedefinirSenhaCommand(request.Email, request.Token, request.NovaSenha), cancellationToken);
        return Ok(new { sucesso = true });
    }

    [HttpPost("email/confirmar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmarEmail(ConfirmarEmailRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ConfirmarEmailCommand(request.Email, request.Token), cancellationToken);
        return Ok(new { sucesso = true });
    }

    [HttpPost("email/reenviar-confirmacao")]
    [ProducesResponseType<MensagemGenericaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReenviarConfirmacaoEmail(ReenviarConfirmacaoEmailRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ReenviarConfirmacaoEmailCommand(request.Email), cancellationToken);
        return Ok(resultado);
    }
}

public sealed record LoginRequest(
    string Email,
    string Senha,
    Guid AplicacaoId,
    string IdentificadorDispositivo,
    string? Navegador,
    string? SistemaOperacional,
    string? TipoDispositivo,
    string? Cidade,
    string? Estado,
    string? Pais,
    decimal? Latitude,
    decimal? Longitude);

public sealed record AtualizarTokenRequest(string TokenAtualizacao);

public sealed record EsqueciSenhaRequest(string Email);

public sealed record RedefinirSenhaRequest(string Email, string Token, string NovaSenha);

public sealed record ConfirmarEmailRequest(string Email, string Token);

public sealed record ReenviarConfirmacaoEmailRequest(string Email);
