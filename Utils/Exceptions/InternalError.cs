using System.Net;

namespace Utils.Exceptions
{
    /// <summary>
    /// InternalError (500 Internal Server Error):
    /// Un "envoltorio" para cuando la base de datos u otro componente interno del sistema falla (caída de conexión, error SQL, etc.).
    /// NOTA: El cliente recibe un mensaje genérico por seguridad, mientras que en los logs internos se guarda el detalle técnico real.
    /// </summary>
    public class InternalError : AppException
    {
        public InternalError(string message) 
            : base(HttpStatusCode.InternalServerError, message) { }

        public InternalError(string message, List<string> details) 
            : base(HttpStatusCode.InternalServerError, message, details) { }

        public InternalError(string message, string detail) 
            : base(HttpStatusCode.InternalServerError, message, detail) { }
    }
}
