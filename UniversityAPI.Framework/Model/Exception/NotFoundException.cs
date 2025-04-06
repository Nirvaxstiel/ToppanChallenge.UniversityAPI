using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message, string? errorCode = null)
            : base(message, StatusCodes.Status404NotFound, errorCode) { }
    }
}