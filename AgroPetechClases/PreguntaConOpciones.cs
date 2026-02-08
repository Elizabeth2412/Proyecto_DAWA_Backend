using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroPetechClases
{
    public class PreguntaConOpciones
    {
        public int Id { get; set; }
        public int EvaluacionId { get; set; }
        public string? Texto { get; set; }
        public string? Estado { get; set; }
        public List<Opcion>? Opciones { get; set; }
    }
}
