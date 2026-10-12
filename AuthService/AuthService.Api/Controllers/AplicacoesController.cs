using AuthService.Application.Aplicacoes.CadastrarAplicacao;
using AuthService.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/aplicacoes")]
public sealed class AplicacoesController(IExecutorRequisicoes sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CadastrarAplicacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cadastrar(CadastrarAplicacaoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new CadastrarAplicacaoCommand(
            request.Nome,
            request.Codigo,
            request.Descricao), cancellationToken);

        return CreatedAtAction(nameof(Cadastrar), new { id = resultado.Id }, resultado);
    }
}

public sealed record CadastrarAplicacaoRequest(
    string Nome,
    string Codigo,
    string? Descricao);
