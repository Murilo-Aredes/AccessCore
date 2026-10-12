using AuthService.Domain.Common;

namespace AuthService.Domain.Perfis;

public sealed class Perfil : Entidade
{
    private readonly List<PerfilPermissao> _permissoes = [];

    private Perfil()
    {
    }

    public Perfil(Guid aplicacaoId, string nome, string codigo, string? descricao, DateTimeOffset dataCriacao)
    {
        AplicacaoId = aplicacaoId;
        Nome = nome;
        Codigo = codigo;
        Descricao = descricao;
        Ativo = true;
        DataCriacao = dataCriacao;
        DataAtualizacao = dataCriacao;
    }

    public Guid AplicacaoId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataAtualizacao { get; private set; }
    public IReadOnlyCollection<PerfilPermissao> Permissoes => _permissoes;
}
