using FluentValidation;

namespace AuthService.Application.Sessoes.ConfirmarSessao;

public sealed class ConfirmarSessaoCommandValidator : AbstractValidator<ConfirmarSessaoCommand>
{
    public ConfirmarSessaoCommandValidator()
    {
        RuleFor(x => x.SessaoUsuarioId).NotEmpty();
        RuleFor(x => x.TokenSessaoPendente).NotEmpty();
    }
}
