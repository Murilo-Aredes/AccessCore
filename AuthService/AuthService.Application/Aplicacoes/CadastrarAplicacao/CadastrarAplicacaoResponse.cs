namespace AuthService.Application.Aplicacoes.CadastrarAplicacao;

public sealed record CadastrarAplicacaoResponse(
    Guid Id,
    string Nome,
    string Codigo,
    string? Descricao,
    bool Ativo);
