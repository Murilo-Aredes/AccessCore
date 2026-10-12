using AuthService.Domain.Usuarios;

namespace AuthService.Application.Common;

public interface IHashSenhaUsuario
{
    string GerarHash(Usuario usuario, string senha);
    bool Verificar(Usuario usuario, string senha);
}
