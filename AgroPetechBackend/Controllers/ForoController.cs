using AgroPetechClases;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForoController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ForoController> _logger;

        public ForoController(IConfiguration config, ILogger<ForoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /* Lo que hace este método en el swagger:
         * 1: Listar todos los posts
        {
          "transaccion": "CONSULTAR_POSTS"
        }
         * 2: Consultar post completo con respuestas
        {
          "id": 1,
          "transaccion": "CONSULTAR_POST_COMPLETO"
        }
         * 3: Consultar posts de un usuario
        {
          "usuarioId": 5,
          "transaccion": "CONSULTAR_POSTS_USUARIO"
        }
         * 4: Buscar posts
        {
          "textoBusqueda": "angular",
          "transaccion": "BUSCAR_POSTS"
        }
         * 5: Posts recientes
        {
          "transaccion": "CONSULTAR_POSTS_RECIENTES"
        }
         */
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
                        resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                        resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
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
                                    Id = Convert.ToInt64(row["Id"]),
                                    Titulo = row["Titulo"]?.ToString(),
                                    Contenido = row["Contenido"]?.ToString(),
                                    UrlImagen = row["UrlImagen"]?.ToString(),
                                    UsuarioId = Convert.ToInt32(row["UsuarioId"]),
                                    NombreAutor = row["NombreAutor"]?.ToString(),
                                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"])
                                };

                                if (row.Table.Columns.Contains("ParentId") && row["ParentId"] != DBNull.Value)
                                {
                                    pub.ParentId = Convert.ToInt64(row["ParentId"]);
                                }

                                if (row.Table.Columns.Contains("RootId") && row["RootId"] != DBNull.Value)
                                {
                                    pub.RootId = Convert.ToInt64(row["RootId"]);
                                }

                                if (row.Table.Columns.Contains("FechaModificacion") && row["FechaModificacion"] != DBNull.Value)
                                {
                                    pub.FechaModificacion = Convert.ToDateTime(row["FechaModificacion"]);
                                }

                                if (row.Table.Columns.Contains("NumeroRespuestas") && row["NumeroRespuestas"] != DBNull.Value)
                                {
                                    pub.NumeroRespuestas = Convert.ToInt32(row["NumeroRespuestas"]);
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
        [HttpPost("SetPublicacionForo")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> SetPublicacionForo([FromBody] PublicacionForo publicacion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(publicacion);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetPublicacionForo",
                    cadenaConexion,
                    publicacion.Transaccion ?? "INSERTAR_POST",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();
                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
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


        [HttpPost("CrearPost")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> CrearPost([FromBody] PublicacionForo publicacion)
        {
            try
            {
                publicacion.Transaccion = "INSERTAR_POST";
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


        [HttpPost("CrearRespuesta")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> CrearRespuesta([FromBody] PublicacionForo publicacion)
        {
            try
            {
                publicacion.Transaccion = "INSERTAR_RESPUESTA";
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
    }
}
