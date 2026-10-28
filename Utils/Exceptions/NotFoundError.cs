using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// NotFoundError (404 Not Found):
    /// Debe ser usado cuando un registro o recurso que se está buscando en la base de datos o sistema no existe.
    /// </summary>
    public class NotFoundError : AppException
    {
        public NotFoundError(string message) 
            : base(HttpStatusCode.NotFound, message) { }

        public NotFoundError(string message, List<string> details) 
            : base(HttpStatusCode.NotFound, message, details) { }

        public NotFoundError(string message, string detail) 
            : base(HttpStatusCode.NotFound, message, detail) { }
    }
}
