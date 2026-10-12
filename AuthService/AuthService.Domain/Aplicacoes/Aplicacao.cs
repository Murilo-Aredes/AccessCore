using AuthService.Domain.Common;

namespace AuthService.Domain.Aplicacoes;

public sealed class Aplicacao : Entidade
{
    private Aplicacao()
    {
    }

    public Aplicacao(string nome, string codigo, string? descricao, DateTimeOffset dataCriacao)
    {
        Nome = nome;
        Codigo = codigo;
        Descricao = descricao;
        Ativo = true;
        DataCriacao = dataCriacao;
        DataAtualizacao = dataCriacao;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataAtualizacao { get; private set; }

    public void Ativar(DateTimeOffset dataAtualizacao)
    {
        Ativo = true;
        DataAtualizacao = dataAtualizacao;
    }

    public void Desativar(DateTimeOffset dataAtualizacao)
    {
        Ativo = false;
        DataAtualizacao = dataAtualizacao;
    }
}
