namespace UniversityAPI.Framework.Model
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, string? errorCode = null)
            : base(message, 400, errorCode) { }
    }
}