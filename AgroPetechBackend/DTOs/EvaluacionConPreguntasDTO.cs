namespace AgroPetechBackend.DTOs
{
    public class EvaluacionConPreguntasDTO
    {
        public int Id { get; set; }
        public int? CursoId { get; set; }
        public string? CursoName { get; set; }
        public string? Titulo { get; set; }
        public string? Modulo { get; set; }
        public int? TotalPreguntas { get; set; }
        public string? Duracion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? Estado { get; set; }
        public List<PreguntaDTO>? Preguntas { get; set; }
    }

    // DTO para las preguntas en la respuesta
    public class PreguntaDTO
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public string? Texto { get; set; }
        public string? Estado { get; set; }
        public List<OpcionDTO>? Opciones { get; set; }
    }

    // DTO para las opciones en formato JSON del SP
    public class OpcionDTO
    {
        public int OpcionId { get; set; }  
        public string? Texto { get; set; }
        public bool EsCorrecta { get; set; }
    }

}
