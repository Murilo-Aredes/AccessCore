using AuthService.Application.Common;

namespace AuthService.Api.Seguranca;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IContextoCorrelacao contextoCorrelacao)
    {
        var identificadorCorrelacao = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(identificadorCorrelacao))
        {
            identificadorCorrelacao = Guid.NewGuid().ToString();
        }

        contextoCorrelacao.IdentificadorCorrelacao = identificadorCorrelacao;
        context.Response.Headers["X-Correlation-ID"] = identificadorCorrelacao;

        await next(context);
    }
}
