using FluentValidation;

namespace AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;

public sealed class AtualizarRedefinicaoSenhaObrigatoriaCommandValidator : AbstractValidator<AtualizarRedefinicaoSenhaObrigatoriaCommand>
{
    public AtualizarRedefinicaoSenhaObrigatoriaCommandValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
    }
}
