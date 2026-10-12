using FluentValidation;

namespace AuthService.Application.Aplicacoes.CadastrarAplicacao;

public sealed class CadastrarAplicacaoCommandValidator : AbstractValidator<CadastrarAplicacaoCommand>
{
    public CadastrarAplicacaoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Descricao).MaximumLength(500);
    }
}
