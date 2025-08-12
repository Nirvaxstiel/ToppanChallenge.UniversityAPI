namespace UniversityAPI.Framework.Model.Exception
{
    using Microsoft.AspNetCore.Http;

    public class UnauthorisedError(string message, string? errorCode = null) : ApiException(message, StatusCodes.Status401Unauthorized, errorCode)
    {
    }
}
