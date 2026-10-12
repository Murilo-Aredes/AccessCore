using FluentValidation;

namespace AuthService.Application.Usuarios.CadastrarUsuario;

public sealed class CadastrarUsuarioCommandValidator : AbstractValidator<CadastrarUsuarioCommand>
{
    public CadastrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
