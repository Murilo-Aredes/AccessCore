using AuthService.Domain.Common;

namespace AuthService.Domain.Permissoes;

public sealed class Permissao : Entidade
{
    private Permissao()
    {
    }

    public Permissao(Guid aplicacaoId, string codigo, string nome, string? descricao, DateTimeOffset dataCriacao)
    {
        AplicacaoId = aplicacaoId;
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        DataCriacao = dataCriacao;
    }

    public Guid AplicacaoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
}
