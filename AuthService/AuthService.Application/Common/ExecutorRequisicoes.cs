using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application.Common;

public sealed class ExecutorRequisicoes(IServiceProvider serviceProvider) : IExecutorRequisicoes
{
    public async Task<TResposta> Send<TResposta>(IRequisicao<TResposta> requisicao, CancellationToken cancellationToken = default)
    {
        var tipoRequisicao = requisicao.GetType();
        var tipoValidador = typeof(IValidator<>).MakeGenericType(tipoRequisicao);
        var validadores = serviceProvider.GetServices(tipoValidador);

        foreach (var validador in validadores.OfType<IValidator>())
        {
            var contexto = new ValidationContext<object>(requisicao);
            var resultadoValidacao = await validador.ValidateAsync(contexto, cancellationToken);
            if (!resultadoValidacao.IsValid)
            {
                throw new ValidationException(resultadoValidacao.Errors);
            }
        }

        var tipoManipulador = typeof(IManipuladorRequisicao<,>).MakeGenericType(tipoRequisicao, typeof(TResposta));
        var manipulador = serviceProvider.GetRequiredService(tipoManipulador);
        var metodoHandle = tipoManipulador.GetMethod(nameof(IManipuladorRequisicao<IRequisicao<TResposta>, TResposta>.Handle))!;
        var tarefa = (Task<TResposta>)metodoHandle.Invoke(manipulador, [requisicao, cancellationToken])!;
        return await tarefa;
    }
}
