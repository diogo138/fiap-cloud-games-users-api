using System.Text.Json;

namespace FCG.Users.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada: {Mensagem}", ex.Message);
            await TratarExcecaoAsync(context, ex);
        }
    }

    private static async Task TratarExcecaoAsync(HttpContext context, Exception ex)
    {
        var (statusCode, mensagem) = ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, ex.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno no servidor.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var resposta = new { status = statusCode, erro = mensagem };
        var json = JsonSerializer.Serialize(resposta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
