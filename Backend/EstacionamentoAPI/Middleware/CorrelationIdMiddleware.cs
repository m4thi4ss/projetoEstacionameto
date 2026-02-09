namespace EstacionamentoAPI.Middleware
{
    /// <summary>
    /// Middleware que adiciona um Correlation ID único para cada requisição
    /// Permite rastrear requisições através de logs
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
        {
            // Tentar obter correlation ID do header, ou gerar um novo
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                              ?? Guid.NewGuid().ToString();

            // Adicionar ao response header
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

            // Adicionar ao contexto de log
            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            }))
            {
                logger.LogInformation(
                    "🔍 Request iniciada: {Method} {Path} | CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId
                );

                var startTime = DateTime.UtcNow;

                try
                {
                    await _next(context);
                }
                finally
                {
                    var elapsedTime = DateTime.UtcNow - startTime;
                    
                    logger.LogInformation(
                        "✅ Request finalizada: {Method} {Path} | Status: {StatusCode} | Duração: {Duration}ms | CorrelationId: {CorrelationId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        elapsedTime.TotalMilliseconds,
                        correlationId
                    );
                }
            }
        }
    }

    /// <summary>
    /// Extension method para facilitar o registro do middleware
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
