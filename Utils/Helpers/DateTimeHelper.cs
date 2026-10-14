namespace Utils.Helpers
{
    public static class DateTimeHelper
    {
        private const string IdZonaIana = "America/Argentina/Buenos_Aires";
        private const string IdZonaWindows = "Argentina Standard Time";

        public static readonly TimeZoneInfo ZonaHoraria = ResolverZonaHoraria();

        private static TimeZoneInfo ResolverZonaHoraria()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(IdZonaIana);
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(IdZonaWindows);
            }
        }

        /// <summary>
        /// Convierte una hora de pared (sin zona) interpretada en la zona horaria local
        /// (America/Argentina/Buenos_Aires) al instante UTC correspondiente.
        /// </summary>
        public static DateTime LocalAUtc(DateTime local)
        {
            var sinEspecificar = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(sinEspecificar, ZonaHoraria);
        }

        /// <summary>
        /// Combina una fecha local con una hora de pared (regla) y devuelve el instante UTC.
        /// </summary>
        public static DateTime LocalAUtc(DateOnly fecha, TimeSpan hora)
        {
            return LocalAUtc(fecha.ToDateTime(TimeOnly.FromTimeSpan(hora)));
        }

        /// <summary>
        /// Convierte un instante UTC a la hora de pared de la zona horaria local
        /// (America/Argentina/Buenos_Aires).
        /// </summary>
        public static DateTime UtcALocal(DateTime utc)
        {
            var utcEspecificado = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utcEspecificado, ZonaHoraria);
        }

        /// <summary>
        /// Normaliza una fecha/hora recibida del API al instante UTC.
        /// - Utc: se deja igual.
        /// - Local: se convierte con ToUniversalTime().
        /// - Unspecified: se interpreta como hora local Argentina y se convierte a UTC.
        /// </summary>
        public static DateTime NormalizarAUtc(DateTime valor)
        {
            return valor.Kind switch
            {
                DateTimeKind.Utc => valor,
                DateTimeKind.Local => valor.ToUniversalTime(),
                _ => LocalAUtc(valor)
            };
        }
    }
}
