using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class ConflictException : ApiException
    {
        public ConflictException(string message, string? errorCode = null)
            : base(message, StatusCodes.Status409Conflict, errorCode) { }
    }
}