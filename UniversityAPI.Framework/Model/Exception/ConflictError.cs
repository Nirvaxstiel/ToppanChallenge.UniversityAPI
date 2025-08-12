namespace UniversityAPI.Framework.Model.Exception
{
    using Microsoft.AspNetCore.Http;

    public class ConflictError(string message, string? errorCode = null) : ApiException(message, StatusCodes.Status409Conflict, errorCode)
    {
    }
}