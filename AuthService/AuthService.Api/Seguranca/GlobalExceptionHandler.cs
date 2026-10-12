using AuthService.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Seguranca;

public sealed class GlobalExceptionHandler(IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, titulo) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Requisição inválida."),
            NaoAutenticadoException => (StatusCodes.Status401Unauthorized, "Não autenticado."),
            AcessoNegadoException => (StatusCodes.Status403Forbidden, "Acesso negado."),
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            ConflitoException => (StatusCodes.Status409Conflict, "Conflito."),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = environment.IsDevelopment() ? exception.Message : ObterMensagemSegura(exception),
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static string ObterMensagemSegura(Exception exception)
    {
        return exception is AuthServiceException ? exception.Message : "Ocorreu um erro ao processar a solicitação.";
    }
}
