namespace AuthService.Domain.Perfis;

public sealed class PerfilPermissao
{
    private PerfilPermissao()
    {
    }

    public PerfilPermissao(Guid perfilId, Guid permissaoId, DateTimeOffset dataCriacao)
    {
        PerfilId = perfilId;
        PermissaoId = permissaoId;
        DataCriacao = dataCriacao;
    }

    public Guid PerfilId { get; private set; }
    public Guid PermissaoId { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
}
