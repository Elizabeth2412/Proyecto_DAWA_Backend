using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroPetechClases
{
    public class Opcion
    {
        public int Id { get; set; }
        public int? PreguntaId { get; set; }
        public string? Texto { get; set; }
        public bool EsCorrecta { get; set; }
        public string? Estado { get; set; }
        public int? UsuarioId { get; set; }
        public string? Transaccion { get; set; }
    }
}
