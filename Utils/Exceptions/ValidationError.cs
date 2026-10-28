using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// ValidationError (400 Bad Request):
    /// Debe ser usado cuando los datos que envía el usuario son incorrectos, faltan campos obligatorios,
    /// o cuando los datos no pasan las reglas de validación y formato requeridas por la aplicación.
    /// </summary>
    public class ValidationError : AppException
    {
        public ValidationError(string message) 
            : base(HttpStatusCode.BadRequest, message) { }

        public ValidationError(string message, List<string> details) 
            : base(HttpStatusCode.BadRequest, message, details) { }

        public ValidationError(string message, string detail) 
            : base(HttpStatusCode.BadRequest, message, detail) { }
    }
}
