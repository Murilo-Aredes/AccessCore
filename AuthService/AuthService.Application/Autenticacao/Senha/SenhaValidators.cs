using FluentValidation;

namespace AuthService.Application.Autenticacao.Senha;

public sealed class SolicitarRecuperacaoSenhaCommandValidator : AbstractValidator<SolicitarRecuperacaoSenhaCommand>
{
    public SolicitarRecuperacaoSenhaCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class RedefinirSenhaCommandValidator : AbstractValidator<RedefinirSenhaCommand>
{
    public RedefinirSenhaCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NovaSenha).NotEmpty().MinimumLength(8);
    }
}
