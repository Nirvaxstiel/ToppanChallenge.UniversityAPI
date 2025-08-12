namespace UniversityAPI.Framework.Model.Exception
{
    using Microsoft.AspNetCore.Http;

    public class NotFoundError(string message, string? errorCode = null) : ApiException(message, StatusCodes.Status404NotFound, errorCode)
    {
    }
}