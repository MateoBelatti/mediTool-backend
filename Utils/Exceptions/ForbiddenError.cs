using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// ForbiddenError (403 Forbidden):
    /// Debe ser usado cuando el usuario está autenticado correctamente, pero no tiene los
    /// permisos o roles necesarios para realizar esa acción específica (ej. un usuario común en un panel de administrador).
    /// </summary>
    public class ForbiddenError : AppException
    {
        public ForbiddenError(string message) 
            : base(HttpStatusCode.Forbidden, message) { }

        public ForbiddenError(string message, List<string> details) 
            : base(HttpStatusCode.Forbidden, message, details) { }

        public ForbiddenError(string message, string detail) 
            : base(HttpStatusCode.Forbidden, message, detail) { }
    }
}
