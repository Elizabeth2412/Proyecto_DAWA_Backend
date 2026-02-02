using AgroPetechClases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EvaluacionController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EvaluacionController> _logger;

        public EvaluacionController(IConfiguration config, ILogger<EvaluacionController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("GetEvaluacion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]

        public async Task<ActionResult<Resultado>> GetEvaluacion([FromBody] Evaluacion evaluacion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");

                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetEvaluacion",
                    cadenaConexion,
                    evaluacion.Transaccion ?? "CONSULTAR_EVALUACION",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if(dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Evaluacion> evaluaciones = new List<Evaluacion>();
                    foreach(DataRow row in dsResultado.Tables[1].Rows)
                    {
                        try
                        {
                            var eva = new Evaluacion
                            {
                                Id = Convert.ToInt32(row["id"]),
                                Cursoid = row["cursoId"] == DBNull.Value? null: Convert.ToInt32(row["cursoId"]),
                                Titulo = row["titulo"]?.ToString(),
                                Modulo = row["modulo"]?.ToString(),
                                TotalPreguntas = Convert.ToInt32(row["totalPreguntas"]),
                                Duracion = row["duracion"]?.ToString(),
                                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                                Estado = row["estado"]?.ToString(),
                            };

                            evaluaciones.Add(eva);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error procesando fila: {ex.Message}");
                        }
                    }

                    resultado.Data = evaluaciones;
                }
                else
                {
                    resultado.Data = new List<Evaluacion>();
                }

                return Ok(resultado);
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Error en GetPublicacionForo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });

            }
        }


        [HttpPost("GetEvaluacionByNombre")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetEvaluacionByNombre([FromBody] Evaluacion evaluacion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");

                if (string.IsNullOrEmpty(cadenaConexion))
                {
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Cadena de conexión no configurada"
                    });
                }

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetEvaluacion",
                    cadenaConexion,
                    "BUSCAR_EVALUACION",
                    xmlParam?.ToString() ?? ""
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Evaluacion> evaluaciones = new();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        evaluaciones.Add(new Evaluacion
                        {
                            Id = Convert.ToInt32(row["id"]),
                            Cursoid = row["cursoId"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(row["cursoId"]),
                            Titulo = row["titulo"]?.ToString(),
                            Modulo = row["modulo"]?.ToString(),
                            TotalPreguntas = Convert.ToInt32(row["totalPreguntas"]),
                            Duracion = row["duracion"]?.ToString(),
                            FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                            Estado = row["estado"]?.ToString()
                        });
                    }

                    resultado.Data = evaluaciones;
                }
                else
                {
                    resultado.Data = new List<Evaluacion>();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetEvaluacionByNombre");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("InsertarEvaluacion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> InsertarEvaluacion([FromBody] Evaluacion evaluacion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                evaluacion.Transaccion = "INSERTAR_EVALUACION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetEvaluacion",
                    cadenaConexion,
                    "INSERTAR_EVALUACION",
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
                _logger.LogError(ex, "Error en InsertarEvaluacion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("ActualizarEvaluacion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarEvaluacion([FromBody] Evaluacion evaluacion)
        {
            try
            {
                if (!evaluacion.Id.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la evaluación es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                evaluacion.Transaccion = "ACTUALIZAR_EVALUACION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetEvaluacion",
                    cadenaConexion,
                    "ACTUALIZAR_EVALUACION",
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
                _logger.LogError(ex, "Error en ActualizarEvaluacion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("EliminarEvaluacion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarEvaluacion([FromBody] Evaluacion evaluacion)
        {
            try
            {
                if (!evaluacion.Id.HasValue)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la evaluación es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                evaluacion.Transaccion = "ELIMINAR_EVALUACION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetEvaluacion",
                    cadenaConexion,
                    "ELIMINAR_EVALUACION",
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
                _logger.LogError(ex, "Error en EliminarEvaluacion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

    }
}
