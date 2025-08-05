using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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
                await HandleExceptionAsync(context, ex, _env);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            var errorCode = "UNKNOWN_ERROR";
            var includeDetails = env.IsDevelopment();

            if (exception is ApiException apiEx)
            {
                statusCode = apiEx.StatusCode;
                errorCode = apiEx.ErrorCode ?? ReasonPhrases.GetReasonPhrase(statusCode);
            }

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