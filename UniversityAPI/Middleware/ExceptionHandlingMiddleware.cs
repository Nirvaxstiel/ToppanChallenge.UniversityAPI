using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using UniversityAPI.Framework.Model;

namespace UniversityAPI.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, "API Exception occurred");
                await HandleExceptionAsync(context, ex, env);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex, env);
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