using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgroPetechBackend.DTOs
{
    public class ArchivoFormRequest
    {
        [FromForm]
        public IFormFile? Archivo { get; set; }

        public string? Descripcion { get; set; }
        public string? Usuario { get; set; }
    }
}
