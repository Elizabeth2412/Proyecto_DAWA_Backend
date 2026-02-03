using AgroPetechClases;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CursoArchivoController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CursoArchivoController> _logger;

        public CursoArchivoController(IConfiguration config, ILogger<CursoArchivoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("GetArchivosPorCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetArchivosPorCurso([FromBody] CursoArchivo cursoArchivo)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                cursoArchivo.Transaccion = "ARCHIVOS_POR_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(cursoArchivo);
                if (xmlParam == null)
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Error al generar XML" });

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetArchivosPorCurso",
                    cadenaConexion,
                    cursoArchivo.Transaccion,
                    xmlParam.ToString()
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";

                    if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                    {
                        List<Archivo> archivos = new List<Archivo>();
                        foreach (DataRow row in dsResultado.Tables[1].Rows)
                        {
                            try
                            {
                                var archivo = new Archivo
                                {
                                    Id = Convert.ToInt32(row["id"]),
                                    Nombre = row["nombre"]?.ToString(),
                                    Tipo = row["tipo"]?.ToString(),
                                    Tamano = row["tamano"] != DBNull.Value ? Convert.ToInt64(row["tamano"]) : null,
                                    Descripcion = row["descripcion"]?.ToString(),
                                    Usuario = row["usuario"]?.ToString(),
                                    Estado = row["estado"]?.ToString(),
                                    FechaSubida = row["fechaSubida"] != DBNull.Value ? Convert.ToDateTime(row["fechaSubida"]) : null
                                };

                                archivos.Add(archivo);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error procesando archivo: {ex.Message}");
                            }
                        }

                        resultado.Data = archivos;
                    }
                    else
                    {
                        resultado.Data = new List<Archivo>();
                    }
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetArchivosPorCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("AgregarArchivoACurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> AgregarArchivoACurso([FromBody] CursoArchivo cursoArchivo)
        {
            try
            {
                if (!cursoArchivo.CursoId.HasValue || !cursoArchivo.ArchivoId.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "CursoId y ArchivoId son obligatorios"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                cursoArchivo.Transaccion = "AGREGAR_ARCHIVO_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(cursoArchivo);
                if (xmlParam == null)
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Error al generar XML" });

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetCursoArchivo",
                    cadenaConexion,
                    cursoArchivo.Transaccion,
                    xmlParam.ToString()
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AgregarArchivoACurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("EliminarArchivoDeCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarArchivoDeCurso([FromBody] CursoArchivo cursoArchivo)
        {
            try
            {
                if (!cursoArchivo.CursoId.HasValue || !cursoArchivo.ArchivoId.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "CursoId y ArchivoId son obligatorios"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                cursoArchivo.Transaccion = "ELIMINAR_ARCHIVO_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(cursoArchivo);
                if (xmlParam == null)
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Error al generar XML" });

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetCursoArchivo",
                    cadenaConexion,
                    cursoArchivo.Transaccion,
                    xmlParam.ToString()
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarArchivoDeCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
    }
}