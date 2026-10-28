using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// UnauthorizedError (401 Unauthorized):
    /// Debe ser usado cuando el usuario necesita estar autenticado para acceder a una ruta
    /// y no envió credenciales válidas (ej. si el token expiró, no es válido o falta por completo).
    /// </summary>
    public class UnauthorizedError : AppException
    {
        public UnauthorizedError(string message) 
            : base(HttpStatusCode.Unauthorized, message) { }

        public UnauthorizedError(string message, List<string> details) 
            : base(HttpStatusCode.Unauthorized, message, details) { }

        public UnauthorizedError(string message, string detail) 
            : base(HttpStatusCode.Unauthorized, message, detail) { }
    }
}
