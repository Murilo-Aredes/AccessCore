using AuthService.Application.Common;
using AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;
using AuthService.Application.Usuarios.CadastrarUsuario;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(IExecutorRequisicoes sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CadastrarUsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Cadastrar(CadastrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new CadastrarUsuarioCommand(
            request.Nome,
            request.Email,
            request.Senha,
            request.EnviarConfirmacaoEmail,
            request.RequerRedefinicaoSenha), cancellationToken);

        return CreatedAtAction(nameof(Cadastrar), new { id = resultado.Id }, resultado);
    }

    [HttpPatch("{id:guid}/redefinicao-senha-obrigatoria")]
    [ProducesResponseType<AtualizarRedefinicaoSenhaObrigatoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AtualizarRedefinicaoSenhaObrigatoria(
        Guid id,
        AtualizarRedefinicaoSenhaObrigatoriaRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new AtualizarRedefinicaoSenhaObrigatoriaCommand(
            id,
            request.RequerRedefinicaoSenha), cancellationToken);

        return Ok(resultado);
    }
}

public sealed record CadastrarUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    bool EnviarConfirmacaoEmail = true,
    bool RequerRedefinicaoSenha = false);

public sealed record AtualizarRedefinicaoSenhaObrigatoriaRequest(bool RequerRedefinicaoSenha);
