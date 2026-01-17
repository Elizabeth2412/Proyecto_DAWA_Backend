namespace AgroPetechClases
{
    public class Curso
    {
        public int? Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Nivel { get; set; }
        public int? Duracion { get; set; }
        public int? Progreso { get; set; }
        public string? Instructor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<Archivo>? Archivos { get; set; }
        public string? Transaccion { get; set; }
    }
}
