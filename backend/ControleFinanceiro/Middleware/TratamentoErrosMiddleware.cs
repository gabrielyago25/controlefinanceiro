using ControleFinanceiro.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.Middleware;

public sealed class TratamentoErrosMiddleware(RequestDelegate next, ILogger<TratamentoErrosMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, titulo) = ex switch
            {
                ValidacaoException or ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
                NaoAutenticadoException => (StatusCodes.Status401Unauthorized, ex.Message),
                NaoEncontradoException => (StatusCodes.Status404NotFound, ex.Message),
                ConflitoException => (StatusCodes.Status409Conflict, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Erro inesperado.")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Falha inesperada ao processar a requisição.");
            }

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
