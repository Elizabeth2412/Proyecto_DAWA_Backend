using AgroPetechClases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CursoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CursoController> _logger;

        public CursoController(IConfiguration config, ILogger<CursoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("GetCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetCurso([FromBody] Curso curso)
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

                // Si no viene transacción, usar LISTAR_CURSOS por defecto
                curso.Transaccion ??= "LISTAR_CURSOS";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(curso);

                if (xmlParam == null)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Error al generar XML"
                    });

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetCurso",
                    cadenaConexion,
                    curso.Transaccion,
                    xmlParam.ToString()
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";

                    // Si hay datos en tabla 1, procesarlos
                    if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                    {
                        List<Curso> cursos = new List<Curso>();
                        foreach (DataRow row in dsResultado.Tables[1].Rows)
                        {
                            try
                            {
                                var cu = new Curso
                                {
                                    Id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : (int?)null,
                                    Titulo = row["titulo"]?.ToString(),
                                    Descripcion = row["descripcion"]?.ToString(),
                                    Nivel = row["nivel"]?.ToString(),
                                    Duracion = row["duracion"] != DBNull.Value ? Convert.ToInt32(row["duracion"]) : (int?)null,
                                    Instructor = row["instructor"]?.ToString(),
                                    FechaCreacion = row["fechaCreacion"] != DBNull.Value ? Convert.ToDateTime(row["fechaCreacion"]) : (DateTime?)null,
                                    FechaActualizacion = row["fechaActualizacion"] != DBNull.Value ? Convert.ToDateTime(row["fechaActualizacion"]) : (DateTime?)null,
                                    Transaccion = row["transaccion"]?.ToString()
                                };

                                cursos.Add(cu);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error procesando fila de curso: {ex.Message}");
                            }
                        }

                        resultado.Data = cursos;
                    }
                    else
                    {
                        resultado.Data = new List<Curso>();
                    }
                }
                else
                {
                    resultado.Respuesta = "Error";
                    resultado.Leyenda = "No se recibió respuesta del servidor de base de datos";
                    resultado.Data = new List<Curso>();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}",
                    Data = new List<Curso>()
                });
            }
        }

        [HttpPost("InsertarCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> InsertarCurso([FromBody] Curso curso)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                curso.Transaccion = "INSERTAR_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(curso);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetCurso",
                    cadenaConexion,
                    "INSERTAR_CURSO",
                    xmlParam?.ToString() ?? ""
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
                _logger.LogError(ex, "Error en InsertarCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("ActualizarCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarCurso([FromBody] Curso curso)
        {
            try
            {
                if (!curso.Id.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la evaluación es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                curso.Transaccion = "ACTUALIZAR_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(curso);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetCurso",
                    cadenaConexion,
                    "ACTUALIZAR_CURSO",
                    xmlParam?.ToString() ?? ""
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
                _logger.LogError(ex, "Error en ActualizarCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("EliminarCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarCurso([FromBody] Curso curso)
        {
            try
            {
                if (!curso.Id.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id del curso es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                curso.Transaccion = "ELIMINAR_CURSO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(curso);

                if (xmlParam == null)
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Error al generar XML" });

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetCurso",
                    cadenaConexion,
                    "ELIMINAR_CURSO",
                    xmlParam.ToString()
                );

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
                _logger.LogError(ex, "Error en EliminarCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

    }
}
