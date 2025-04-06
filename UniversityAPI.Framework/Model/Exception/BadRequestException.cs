using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, string? errorCode = null)
            : base(message, StatusCodes.Status400BadRequest, errorCode) { }
    }
}