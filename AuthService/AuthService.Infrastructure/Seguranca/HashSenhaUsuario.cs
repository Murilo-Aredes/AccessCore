using AuthService.Application.Common;
using AuthService.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Seguranca;

public sealed class HashSenhaUsuario : IHashSenhaUsuario
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string GerarHash(Usuario usuario, string senha)
    {
        return _passwordHasher.HashPassword(usuario, senha);
    }

    public bool Verificar(Usuario usuario, string senha)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
