using AuthService.Application.Common;
using AuthService.Domain.Aplicacoes;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Aplicacoes.CadastrarAplicacao;

public sealed class CadastrarAplicacaoCommandHandler(
    IAuthDbContext dbContext,
    IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<CadastrarAplicacaoCommand, CadastrarAplicacaoResponse>
{
    public async Task<CadastrarAplicacaoResponse> Handle(CadastrarAplicacaoCommand requisicao, CancellationToken cancellationToken)
    {
        var codigo = requisicao.Codigo.Trim().ToUpperInvariant();

        var codigoJaCadastrado = await dbContext.Aplicacoes
            .AnyAsync(x => x.Codigo == codigo, cancellationToken);

        if (codigoJaCadastrado)
        {
            throw new ConflitoException("Já existe uma aplicação cadastrada com este código.");
        }

        var descricao = string.IsNullOrWhiteSpace(requisicao.Descricao)
            ? null
            : requisicao.Descricao.Trim();

        var aplicacao = new Aplicacao(
            requisicao.Nome.Trim(),
            codigo,
            descricao,
            provedorDataHora.UtcNow);

        dbContext.Aplicacoes.Add(aplicacao);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CadastrarAplicacaoResponse(
            aplicacao.Id,
            aplicacao.Nome,
            aplicacao.Codigo,
            aplicacao.Descricao,
            aplicacao.Ativo);
    }
}
