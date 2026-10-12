using FluentValidation;

namespace AuthService.Application.Sessoes.RejeitarSessao;

public sealed class RejeitarSessaoCommandValidator : AbstractValidator<RejeitarSessaoCommand>
{
    public RejeitarSessaoCommandValidator()
    {
        RuleFor(x => x.SessaoUsuarioId).NotEmpty();
        RuleFor(x => x.TokenSessaoPendente).NotEmpty();
    }
}
