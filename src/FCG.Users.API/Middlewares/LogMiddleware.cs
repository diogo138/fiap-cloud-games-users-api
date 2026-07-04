using System.Diagnostics;

namespace FCG.Users.API.Middlewares;

public class LogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogMiddleware> _logger;

    public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("[REQ] {Method} {Path}", method, path);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation(
            "[RES] {Method} {Path} | Status: {StatusCode} | Duração: {ElapsedMs}ms",
            method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
