using AuthService.Application.Common;

namespace AuthService.Application.Aplicacoes.CadastrarAplicacao;

public sealed record CadastrarAplicacaoCommand(
    string Nome,
    string Codigo,
    string? Descricao) : IRequisicao<CadastrarAplicacaoResponse>;
