using AuthService.Application.Auditorias.ObterAuditoriaEntidade;
using AuthService.Application.Auditorias.ObterAuditorias;
using AuthService.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/auditorias")]
public sealed class AuditoriasController(IExecutorRequisicoes sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<AuditoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(
        [FromQuery] Guid? usuarioId,
        [FromQuery] string? nomeEntidade,
        [FromQuery] DateTimeOffset? dataInicio,
        [FromQuery] DateTimeOffset? dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObterAuditoriasQuery(usuarioId, nomeEntidade, dataInicio, dataFim), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{entidade}/{id}")]
    [ProducesResponseType<IReadOnlyCollection<AuditoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorEntidade(string entidade, string id, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObterAuditoriaEntidadeQuery(entidade, id), cancellationToken);
        return Ok(resultado);
    }
}
