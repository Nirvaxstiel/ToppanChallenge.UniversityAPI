using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class BadRequestError : ApiException
    {
        public BadRequestError(string message, string? errorCode = null)
            : base(message, StatusCodes.Status400BadRequest, errorCode) { }
    }
}