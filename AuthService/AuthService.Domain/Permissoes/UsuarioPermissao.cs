using AuthService.Domain.Common;

namespace AuthService.Domain.Permissoes;

public sealed class UsuarioPermissao : Entidade
{
    private UsuarioPermissao()
    {
    }

    public UsuarioPermissao(Guid usuarioId, Guid permissaoId, TipoPermissaoUsuario tipoPermissao, DateTimeOffset dataCriacao)
    {
        UsuarioId = usuarioId;
        PermissaoId = permissaoId;
        TipoPermissao = tipoPermissao;
        DataCriacao = dataCriacao;
    }

    public Guid UsuarioId { get; private set; }
    public Guid PermissaoId { get; private set; }
    public TipoPermissaoUsuario TipoPermissao { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }

    public void Permitir() => TipoPermissao = TipoPermissaoUsuario.Permitir;

    public void Negar() => TipoPermissao = TipoPermissaoUsuario.Negar;
}
