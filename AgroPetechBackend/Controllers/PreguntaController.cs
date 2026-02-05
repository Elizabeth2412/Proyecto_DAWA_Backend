using AgroPetechClases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreguntaController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PreguntaController> _logger;

        public PreguntaController(IConfiguration config, ILogger<PreguntaController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("GetPreguntas")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetPreguntas()
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

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetPregunta",
                    cadenaConexion,
                    "CONSULTAR_PREGUNTAS",
                    ""
                );

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Pregunta> preguntas = new();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        preguntas.Add(new Pregunta
                        {
                            Id = Convert.ToInt32(row["id"]),
                            EvaluacionId = Convert.ToInt32(row["evaluacionId"]),
                            Texto = row["texto"]?.ToString(),
                            Estado = row["estado"]?.ToString()
                        });
                    }

                    resultado.Data = preguntas;
                }
                else
                {
                    resultado.Data = null;
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetPreguntas");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }



        [HttpPost("GetPreguntasPorEvaluacion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetPreguntasPorEvaluacion([FromBody] Pregunta pregunta)
        {
            try
            {
                if (pregunta.EvaluacionId <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la evaluación es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                pregunta.Transaccion = "PREGUNTAS_POR_EVALUACION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(pregunta);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetPregunta",
                    cadenaConexion,
                    pregunta.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Pregunta> preguntas = new();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        preguntas.Add(new Pregunta
                        {
                            Id = Convert.ToInt32(row["id"]),
                            EvaluacionId = Convert.ToInt32(row["evaluacionId"]),
                            Texto = row["texto"]?.ToString(),
                            Estado = row["estado"]?.ToString()
                        });
                    }

                    resultado.Data = preguntas;
                }
                else
                {
                    resultado.Data = new List<Pregunta>();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetPreguntasPorEvaluacion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("GetPreguntaById")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetPreguntaById([FromBody] Pregunta pregunta)
        {
            try
            {
                if (pregunta.Id <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la pregunta es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                pregunta.Transaccion = "BUSCAR_PREGUNTA";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(pregunta);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetPregunta",
                    cadenaConexion,
                    pregunta.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    DataRow row = dsResultado.Tables[1].Rows[0];

                    resultado.Data = new Pregunta
                    {
                        Id = Convert.ToInt32(row["id"]),
                        EvaluacionId = Convert.ToInt32(row["evaluacionId"]),
                        Texto = row["texto"]?.ToString(),
                        Estado = row["estado"]?.ToString()
                    };
                }
                else
                {
                    resultado.Data = null;
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetPreguntaById");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("InsertarPregunta")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> InsertarPregunta([FromBody] Pregunta pregunta)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                pregunta.Transaccion = "INSERTAR_PREGUNTA";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(pregunta);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetPregunta",
                    cadenaConexion,
                    pregunta.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                if (resultado.Respuesta == "Ok" && dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    int idGenerado = Convert.ToInt32(dsResultado.Tables[1].Rows[0]["Id"]);

                    resultado.Data = new Pregunta
                    {
                        Id = idGenerado,
                        EvaluacionId = pregunta.EvaluacionId,
                        Texto = pregunta.Texto,
                        Estado = "Activa"
                    };
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en InsertarPregunta");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("ActualizarPregunta")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarPregunta([FromBody] Pregunta pregunta)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                pregunta.Transaccion = "ACTUALIZAR_PREGUNTA";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(pregunta);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetPregunta",
                    cadenaConexion,
                    pregunta.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarPregunta");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("EliminarPregunta")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarPregunta([FromBody] Pregunta pregunta)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                pregunta.Transaccion = "ELIMINAR_PREGUNTA";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(pregunta);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetPregunta",
                    cadenaConexion,
                    pregunta.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarPregunta");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

    }
}
