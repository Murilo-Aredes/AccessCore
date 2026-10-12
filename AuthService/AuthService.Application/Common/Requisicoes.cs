namespace AuthService.Application.Common;

public interface IRequisicao<out TResposta>
{
}

public interface IManipuladorRequisicao<in TRequisicao, TResposta>
    where TRequisicao : IRequisicao<TResposta>
{
    Task<TResposta> Handle(TRequisicao requisicao, CancellationToken cancellationToken);
}

public interface IExecutorRequisicoes
{
    Task<TResposta> Send<TResposta>(IRequisicao<TResposta> requisicao, CancellationToken cancellationToken = default);
}
