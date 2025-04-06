namespace UniversityAPI.Framework.Model
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }

        public ApiException(string message, int statusCode = 500, string? errorCode = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public ApiException() : base()
        {
        }

        public ApiException(string? message) : base(message)
        {
        }

        public ApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}