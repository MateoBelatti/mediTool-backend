namespace Utils.DTOs.Documento
{
    public class DocumentoSubidaDto
    {
        public Stream Contenido { get; set; } = Stream.Null;
        public string NombreArchivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long TamanoBytes { get; set; }
    }
}
