namespace UniversityAPI.Framework.Model.Exception
{
    using Microsoft.AspNetCore.Http;

    public class BadRequestError(string message, string? errorCode = null) : ApiException(message, StatusCodes.Status400BadRequest, errorCode)
    {
    }
}