namespace UniversityAPI.Framework.Model
{
    public class ConflictException : ApiException
    {
        public ConflictException(string message, string? errorCode = null)
            : base(message, 409, errorCode) { }
    }
}