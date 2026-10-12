using FluentValidation;

namespace AuthService.Application.Autenticacao.Email;

public sealed class ConfirmarEmailCommandValidator : AbstractValidator<ConfirmarEmailCommand>
{
    public ConfirmarEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class ReenviarConfirmacaoEmailCommandValidator : AbstractValidator<ReenviarConfirmacaoEmailCommand>
{
    public ReenviarConfirmacaoEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
