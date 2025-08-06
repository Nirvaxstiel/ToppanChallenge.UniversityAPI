using Microsoft.AspNetCore.Http;

namespace UniversityAPI.Framework.Model.Exception
{
    public class UnauthorisedError : ApiException
    {
        public UnauthorisedError(string message, string? errorCode = null)
              : base(message, StatusCodes.Status401Unauthorized, errorCode) { }
    }
}
