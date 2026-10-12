using FluentValidation;

namespace AuthService.Application.Autenticacao.RealizarLogin;

public sealed class RealizarLoginCommandValidator : AbstractValidator<RealizarLoginCommand>
{
    public RealizarLoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty();
        RuleFor(x => x.AplicacaoId).NotEmpty();
        RuleFor(x => x.IdentificadorDispositivo).NotEmpty().MaximumLength(200);
    }
}
