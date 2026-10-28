using System.Net;

namespace Utils.Exceptions
{
    public class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public List<string> Details { get; } = new();

        public AppException(HttpStatusCode statusCode, string message) 
            : base(message)
        {
            StatusCode = statusCode;
        }

        public AppException(HttpStatusCode statusCode, string message, List<string> details) 
            : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }

        public AppException(HttpStatusCode statusCode, string message, string detail) 
            : base(message)
        {
            StatusCode = statusCode;
            Details = new List<string> { detail };
        }
    }
}
