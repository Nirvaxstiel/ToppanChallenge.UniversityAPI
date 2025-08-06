using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class NotFoundError : ApiException
    {
        public NotFoundError(string message, string? errorCode = null)
            : base(message, StatusCodes.Status404NotFound, errorCode) { }
    }
}