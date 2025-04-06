using System.Text.Json;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API Exception occurred");
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            var errorCode = "UNKNOWN_ERROR";
            var includeDetails = false;

            if (exception is ApiException apiEx)
            {
                statusCode = apiEx.StatusCode;
                errorCode = apiEx.ErrorCode;
            }
            //#if DEBUG
            //includeDetails = true;
            //#endif

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                Status = statusCode,
                ErrorCode = errorCode,
                Message = exception.Message,
                Details = includeDetails ? exception.ToString() : null
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}