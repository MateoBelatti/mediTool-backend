using Biblioteca.Entities;
using Biblioteca.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Almacenamiento
{
    public class ArchivoStorage : IArchivoStorage
    {
        private readonly ApplicationDbContext _context;

        public ArchivoStorage(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ArchivoGuardado> GuardarAsync(Stream contenido, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contenido, nameof(contenido));

            using var buffer = new MemoryStream();
            await contenido.CopyToAsync(buffer, cancellationToken);
            var bytes = buffer.ToArray();

            var archivo = new Archivo
            {
                Clave = Guid.NewGuid().ToString("N"),
                Contenido = bytes,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Archivos.AddAsync(archivo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new ArchivoGuardado(archivo.Clave, bytes.LongLength);
        }

        public async Task<Stream?> AbrirAsync(string clave, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clave, nameof(clave));

            var archivo = await _context.Archivos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Clave == clave, cancellationToken);

            if (archivo == null) return null;

            return new MemoryStream(archivo.Contenido);
        }

        public async Task EliminarAsync(string clave, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(clave, nameof(clave));

            var archivo = await _context.Archivos
                .FirstOrDefaultAsync(a => a.Clave == clave, cancellationToken);

            if (archivo == null) return;

            _context.Archivos.Remove(archivo);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
