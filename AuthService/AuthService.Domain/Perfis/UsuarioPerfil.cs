namespace AuthService.Domain.Perfis;

public sealed class UsuarioPerfil
{
    private UsuarioPerfil()
    {
    }

    public UsuarioPerfil(Guid usuarioId, Guid perfilId, DateTimeOffset dataCriacao)
    {
        UsuarioId = usuarioId;
        PerfilId = perfilId;
        DataCriacao = dataCriacao;
    }

    public Guid UsuarioId { get; private set; }
    public Guid PerfilId { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }
}
