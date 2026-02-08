using AgroPetechBackend.DTOs;
using AgroPetechBackend.Services;
using AgroPetechClases;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ForoController> _logger;
        private readonly MinioService _minioService;

        public ForoController(IConfiguration config, ILogger<ForoController> logger, MinioService minioService)
        {
            _config = config;
            _logger = logger;
            _minioService = minioService;

        }

        [HttpPost("GetPublicacionForo")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetPublicacionForo([FromBody] PublicacionForo publicacion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(publicacion);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetPublicacionForo",
                    cadenaConexion,
                    publicacion.Transaccion ?? "CONSULTAR_POSTS",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    // Tabla 0: Respuesta y Leyenda
                    if (dsResultado.Tables[0].Rows.Count > 0)
                    {
                        resultado.Respuesta = dsResultado.Tables[0].Rows[0]["respuesta"]?.ToString() ?? "Error";
                        resultado.Leyenda = dsResultado.Tables[0].Rows[0]["leyenda"]?.ToString() ?? "Sin mensaje";
                    }

                    // Tabla 1: Datos de publicaciones
                    if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                    {
                        List<PublicacionForo> publicaciones = new List<PublicacionForo>();
                        foreach (DataRow row in dsResultado.Tables[1].Rows)
                        {
                            try
                            {
                                var pub = new PublicacionForo
                                {
                                    Id = Convert.ToInt64(row["id"]),
                                    Titulo = row["titulo"]?.ToString(),
                                    Contenido = row["contenido"]?.ToString(),
                                    UrlImagen = row["urlImagen"]?.ToString(),
                                    UsuarioId = Convert.ToInt32(row["usuarioId"]),
                                    NombreAutor = row["nombreAutor"]?.ToString(),
                                    FechaCreacion = Convert.ToDateTime(row["fechaCreacion"])
                                };

                                if (row.Table.Columns.Contains("parentId") && row["parentId"] != DBNull.Value)
                                {
                                    pub.ParentId = Convert.ToInt64(row["parentId"]);
                                }

                                if (row.Table.Columns.Contains("rootId") && row["rootId"] != DBNull.Value)
                                {
                                    pub.RootId = Convert.ToInt64(row["rootId"]);
                                }

                                if (row.Table.Columns.Contains("fechaModificacion") && row["fechaModificacion"] != DBNull.Value)
                                {
                                    pub.FechaModificacion = Convert.ToDateTime(row["fechaModificacion"]);
                                }

                                if (row.Table.Columns.Contains("numeroRespuestas") && row["numeroRespuestas"] != DBNull.Value)
                                {
                                    pub.NumeroRespuestas = Convert.ToInt32(row["numeroRespuestas"]);
                                }
                                if (row.Table.Columns.Contains("usuarioModificacionId") && row["usuarioModificacionId"] != DBNull.Value)
                                {
                                    pub.UsuarioModificacionId = Convert.ToInt32(row["usuarioModificacionId"]);
                                }

                                publicaciones.Add(pub);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error procesando fila: {ex.Message}");
                            }
                        }
                        resultado.Data = publicaciones;
                    }
                    else
                    {
                        resultado.Data = new List<PublicacionForo>();
                    }
                }
                else
                {
                    resultado.Respuesta = "Error";
                    resultado.Leyenda = "No se recibieron datos del servidor";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetPublicacionForo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        /* Lo que hace este método en el swagger:
         * 1: Crear post
        {
          "titulo": "¿Cómo implementar Guards en Angular?",
          "contenido": "Necesito ayuda con guards...",
          "urlImagen": "https://ejemplo.com/imagen.jpg",
          "usuarioId": 5,
          "transaccion": "INSERTAR_POST"
        }
         * 2: Crear respuesta
        {
          "parentId": 15,
          "contenido": "Puedes usar CanActivate...",
          "usuarioId": 8,
          "transaccion": "INSERTAR_RESPUESTA"
        }
         * 3: Actualizar publicación
        {
          "id": 15,
          "titulo": "¿Cómo implementar Guards en Angular 18?",
          "contenido": "Necesito ayuda actualizada...",
          "usuarioModificacionId": 5,
          "transaccion": "ACTUALIZAR_PUBLICACION"
        }
         * 4: Eliminar publicación
        {
          "id": 15,
          "usuarioEliminacionId": 5,
          "transaccion": "ELIMINAR_PUBLICACION"
        }
         */
        // [HttpPost("SetPublicacionForo")] Como se privatizo el metodo, este endpoint ya no es accesible directamente.
        // Solo se puede usar a través de los endpoints específicos como CrearPost, CrearRespuesta, ActualizarPost y EliminarPost.
        private async Task<ActionResult<Resultado>> SetPublicacionForo(PublicacionForo publicacionForo)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Cadena de conexión no configurada"
                    });

                // Obtener el XML a partir del objeto PublicacionForo
                XDocument xmlParam = Shared.DBXmlMethods.GetXml(publicacionForo);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetPublicacionForo",
                    cadenaConexion,
                    publicacionForo.Transaccion ?? "INSERTAR_POST",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["leyenda"]?.ToString() ?? "Sin mensaje";
                }
                else
                {
                    resultado.Respuesta = "Error";
                    resultado.Leyenda = "No se recibió respuesta del servidor";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SetPublicacionForo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        /* Lo que hace este método en el swagger:
         * Endpoint simplificado para crear un post
        {
          "titulo": "¿Cómo implementar Guards en Angular?",
          "contenido": "Necesito ayuda con guards...",
          "urlImagen": "https://ejemplo.com/imagen.jpg",
          "usuarioId": 5
        }
         */
        [HttpPost("CrearPost")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> CrearPost([FromForm] PublicacionForo publicacion, [FromForm] ArchivoFormRequest? archivo)
        {
            try
            {
                publicacion.Transaccion = "INSERTAR_POST";
                publicacion.UrlImagen = await subirImagen(archivo);
                return await SetPublicacionForo(publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearPost");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        /* Lo que hace este método en el swagger:
         * Endpoint simplificado para crear una respuesta
        {
          "parentId": 15,
          "contenido": "Puedes usar CanActivate...",
          "usuarioId": 8
        }
         */
        [HttpPost("CrearRespuesta")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> CrearRespuesta([FromForm] PublicacionForo publicacion)
        {
            try
            {
                publicacion.Transaccion = "INSERTAR_RESPUESTA";
                publicacion.UrlImagen = null; // Las respuestas no pueden tener imagen
                return await SetPublicacionForo(publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearRespuesta");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        //Actualizar un post existente con opción de cambiar imagen
        [HttpPost("ActualizarPost")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarPost(
            [FromForm] PublicacionForo publicacion,
            [FromForm] ArchivoFormRequest? archivo)
        {
            try
            {
                // Validaciones básicas
                if (publicacion.Id <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "ID de publicación inválido"
                    });

                if (publicacion.UsuarioModificacionId == null || publicacion.UsuarioModificacionId <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Usuario de modificación inválido"
                    });

                // CASO 1: Usuario quiere ELIMINAR la imagen
                if (archivo?.EliminarImagen == true)
                {
                    await EliminarImagenGuardada(publicacion.UrlImagen);
                    publicacion.UrlImagen = null;
                }

                // CASO 2: Usuario quiere REEMPLAZAR la imagen
                else if (archivo?.Archivo != null && archivo.Archivo.Length > 0)
                {
                    string? nuevaUrl = await subirImagen(archivo);

                    // Eliminar imagen anterior si existe
                    await EliminarImagenGuardada(publicacion.UrlImagen);

                    // Asignar nueva URL
                    publicacion.UrlImagen = nuevaUrl;
                }

                // CASO 3: NO tocar imagen (no hacer nada)

                publicacion.Transaccion = "ACTUALIZAR_PUBLICACION";
                return await SetPublicacionForo(publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar post");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        // Eliminar un post y sus respuestas (eliminación lógica)
        [HttpPost("EliminarPost")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarPost([FromBody] PublicacionForo publicacion)
        {
            try
            {
                // Validaciones
                if (publicacion.Id <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "ID de publicación inválido"
                    });

                if (publicacion.UsuarioEliminacionId == null || publicacion.UsuarioEliminacionId <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Usuario de eliminación inválido"
                    });

                // Eliminar imagen física de MinIO
                await EliminarImagenGuardada(publicacion.UrlImagen);
                publicacion.UrlImagen = null;
                publicacion.Transaccion = "ELIMINAR_PUBLICACION";
                return await SetPublicacionForo(publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar post");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        // Método auxiliar para extraer el objectName de una URL de Minio
        private string ExtraerObjectNameDeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var bucket = _config["Minio:Bucket"];
                return uri.AbsolutePath.Replace($"/{bucket}/", "");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error al extraer objectName de URL: {ex.Message}");
                return "";
            }
        }

        // Método auxiliar para validar imagen
        private bool EsImagenValida(IFormFile archivo, out string mensajeError)
        {
            mensajeError = "";

            if (archivo == null || archivo.Length == 0)
            {
                mensajeError = "Archivo vacío";
                return false;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                mensajeError = "Tipo de archivo no permitido. Use: jpg, jpeg, png, gif, webp";
                return false;
            }

            return true;
        }

        // Método auxiliar para subir imagen de MinIO
        private async Task<string?> subirImagen(ArchivoFormRequest? archivo)
        {
            var imagen = archivo?.Archivo;
            string mensajeError = "";
            if (imagen == null || imagen.Length == 0) return null;

            // Verificar que el archivo es una imagen válida
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                mensajeError = "Tipo de archivo no permitido. Use: jpg, jpeg, png, gif, webp";
                return null;
            }

            var urlGenerada = await _minioService.SubirArchivoAsync(archivo.Archivo, "foros");
            return urlGenerada;
        }

        // Método auxiliar para eliminar imagen de MinIO
        private async Task<bool> EliminarImagenGuardada(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                var objectName = ExtraerObjectNameDeUrl(url);
                if (string.IsNullOrEmpty(objectName))
                    return false;

                await _minioService.EliminarArchivoAsync(objectName);
                _logger.LogInformation($"Imagen eliminada de MinIO: {url}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"No se pudo eliminar imagen de MinIO: {ex.Message}");
                return false;
            }
        }
    }
}