using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// ConflictError (409 Conflict):
    /// Debe ser usado para colisiones lógicas de negocio, como intentar registrar un email
    /// o nombre de usuario que ya está guardado en la base de datos.
    /// </summary>
    public class ConflictError : AppException
    {
        public ConflictError(string message) 
            : base(HttpStatusCode.Conflict, message) { }

        public ConflictError(string message, List<string> details) 
            : base(HttpStatusCode.Conflict, message, details) { }

        public ConflictError(string message, string detail) 
            : base(HttpStatusCode.Conflict, message, detail) { }
    }
}
