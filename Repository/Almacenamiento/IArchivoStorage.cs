namespace Repository.Almacenamiento
{
    public interface IArchivoStorage
    {
        Task<ArchivoGuardado> GuardarAsync(Stream contenido, CancellationToken cancellationToken = default);
        Task<Stream?> AbrirAsync(string clave, CancellationToken cancellationToken = default);
        Task EliminarAsync(string clave, CancellationToken cancellationToken = default);
    }

    public record ArchivoGuardado(string Clave, long TamanoBytes);
}
