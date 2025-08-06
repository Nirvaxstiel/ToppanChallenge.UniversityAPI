using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class ConflictError : ApiException
    {
        public ConflictError(string message, string? errorCode = null)
            : base(message, StatusCodes.Status409Conflict, errorCode) { }
    }
}