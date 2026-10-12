using AuthService.Application.Autorizacao.ObterAcessosUsuario;
using AuthService.Application.Common;
using AuthService.Application.Usuarios.ObterSessoesUsuario;
using AuthService.Application.Usuarios.ObterUsuarioAtual;
using AuthService.Application.Usuarios.RevogarSessoesUsuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/usuario-atual")]
public sealed class UsuarioAtualController(IExecutorRequisicoes sender, IUsuarioAtual usuarioAtual) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UsuarioAtualResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        var resultado = await sender.Send(new ObterUsuarioAtualQuery(usuarioId, usuarioAtual.SessaoUsuarioId, usuarioAtual.AplicacaoId), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("acessos")]
    [ProducesResponseType<ObterAcessosUsuarioResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAcessos([FromQuery] Guid aplicacaoId, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObterAcessosUsuarioQuery(ObterUsuarioId(), aplicacaoId), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("sessoes")]
    [ProducesResponseType<IReadOnlyCollection<SessaoUsuarioResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterSessoes(CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObterSessoesUsuarioQuery(ObterUsuarioId()), cancellationToken);
        return Ok(resultado);
    }

    [HttpDelete("sessoes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevogarOutrasSessoes(CancellationToken cancellationToken)
    {
        var quantidade = await sender.Send(new RevogarSessoesUsuarioCommand(
            ObterUsuarioId(),
            usuarioAtual.SessaoUsuarioId,
            "Revogação de todas as outras sessões solicitada pelo usuário.",
            usuarioAtual.EnderecoIp), cancellationToken);

        return Ok(new { quantidadeRevogada = quantidade });
    }

    private Guid ObterUsuarioId()
    {
        return usuarioAtual.UsuarioId ?? throw new NaoAutenticadoException("Usuário não identificado.");
    }
}
