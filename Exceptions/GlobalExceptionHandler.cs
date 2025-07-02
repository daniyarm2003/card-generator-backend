using Microsoft.AspNetCore.Diagnostics;

namespace CardGeneratorBackend.Exceptions
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> mLogger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            string message = exception.Message;
            int statusCode = exception switch
            {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var messageJson = new Dictionary<string, string>
            {
                { "message", message }
            };

            if(statusCode >= 500)
            {
                mLogger.LogError(exception, "Unexpected Exception: {ErrorMessage}", message);
            }
            else
            {
                mLogger.LogWarning("User Request Failed: {Reason}", message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(messageJson, cancellationToken);

            return await ValueTask.FromResult(true);
        }
    }
}
