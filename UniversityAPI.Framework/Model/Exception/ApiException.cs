namespace UniversityAPI.Framework.Model.Exception
{
    public class ApiException : System.Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }

        public ApiException(string message, int statusCode, string? errorCode = null)
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

        public ApiException(string? message, System.Exception? innerException) : base(message, innerException)
        {
        }
    }
}