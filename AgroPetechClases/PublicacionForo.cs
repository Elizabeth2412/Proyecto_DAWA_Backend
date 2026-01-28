namespace AgroPetechClases
{
    public class PublicacionForo
    {
        public long? Id { get; set; }
        public long? ParentId { get; set; }
        public long? RootId { get; set; }
        public string? Titulo { get; set; }
        public string? Contenido { get; set; }
        public string? UrlImagen { get; set; }
        public int? UsuarioId { get; set; }
        public string? NombreAutor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? FechaEliminacion { get; set; }
        public int? UsuarioEliminacionId { get; set; }
        public bool Estado { get; set; } = true;
        public int? NumeroRespuestas { get; set; }
        public string? TextoBusqueda { get; set; }
        public string? Transaccion { get; set; }
    }
}
