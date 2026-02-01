using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroPetechClases
{
    public class Pregunta
    {
        public int Id { get; set; }
        public int? EvaluacionId { get; set; }
        public string? Texto { get; set; }
        public string? Estado { get; set; }
        public int? UsuarioId { get; set; }
        //public ICollection<Opcion> Opciones { get; set; } = new List<Opcion>();
        public string? Transaccion { get; set; }
    }
}
