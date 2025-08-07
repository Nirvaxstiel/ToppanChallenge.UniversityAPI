namespace UniversityAPI.Framework.Model
{
    public class ApiException : System.Exception
    {
        public int StatusCode { get; }
        public string? ErrorCode { get; }

        public ApiException(string message, int statusCode, string? errorCode = null)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.ErrorCode = errorCode;
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