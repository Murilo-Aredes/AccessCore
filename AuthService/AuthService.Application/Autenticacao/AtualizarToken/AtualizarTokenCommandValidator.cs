using FluentValidation;

namespace AuthService.Application.Autenticacao.AtualizarToken;

public sealed class AtualizarTokenCommandValidator : AbstractValidator<AtualizarTokenCommand>
{
    public AtualizarTokenCommandValidator()
    {
        RuleFor(x => x.TokenAtualizacao).NotEmpty();
    }
}
