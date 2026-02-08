using AgroPetechBackend.DTOs;
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
                                CursoId = row["cursoId"] == DBNull.Value? null: Convert.ToInt32(row["cursoId"]),
                                CursoName = row["cursoName"]?.ToString(),
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
                            CursoId = row["cursoId"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(row["cursoId"]),
                            CursoName = row["cursoName"]?.ToString(),
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

        [HttpPost("GetEvaluacionByCurso")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetEvaluacionByCurso([FromBody] Evaluacion evaluacion)
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

                // Crear el XML con el cursoId
                XDocument xmlParam = Shared.DBXmlMethods.GetXml(evaluacion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetEvaluacion",
                    cadenaConexion,
                    "EVALUACION_POR_CURSO",
                    xmlParam?.ToString() ?? ""
                );

                var resultado = new Resultado();


                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                // Procesar evaluaciones (Tabla 1)
                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Evaluacion> evaluaciones = new();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        try
                        {
                            evaluaciones.Add(new Evaluacion
                            {
                                Id = Convert.ToInt32(row["id"]),
                                CursoId = row["cursoId"] == DBNull.Value
                                    ? null
                                    : Convert.ToInt32(row["cursoId"]),
                                CursoName = row["cursoName"]?.ToString(),
                                Titulo = row["titulo"]?.ToString(),
                                Modulo = row["modulo"]?.ToString(),
                                TotalPreguntas = Convert.ToInt32(row["totalPreguntas"]),
                                Duracion = row["duracion"]?.ToString(),
                                FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                                Estado = row["estado"]?.ToString()
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error procesando fila de evaluación: {ex.Message}");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetEvaluacionByCurso");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("GetEvaluacionConPreguntas")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetEvaluacionConPreguntas([FromBody] Evaluacion evaluacion)
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
                    "EVALUACION_CON_PREGUNTAS",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                // Tabla 0: Respuesta y Leyenda
                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                // Tabla 1: Datos de la evaluación
                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    DataRow row = dsResultado.Tables[1].Rows[0];

                    var evaluacionDTO = new EvaluacionConPreguntasDTO
                    {
                        Id = Convert.ToInt32(row["id"]),
                        CursoId = row["cursoId"] == DBNull.Value ? null : Convert.ToInt32(row["cursoId"]),
                        CursoName = row["cursoName"]?.ToString(),
                        Titulo = row["titulo"]?.ToString(),
                        Modulo = row["modulo"]?.ToString(),
                        TotalPreguntas = Convert.ToInt32(row["totalPreguntas"]),
                        Duracion = row["duracion"]?.ToString(),
                        FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                        Estado = row["estado"]?.ToString(),
                        Preguntas = new List<PreguntaDTO>()
                    };

                    if (dsResultado.Tables.Count > 2 && dsResultado.Tables[2].Rows.Count > 0)
                    {
                        _logger.LogInformation($"Número de preguntas: {dsResultado.Tables[2].Rows.Count}");

                        foreach (DataRow preguntaRow in dsResultado.Tables[2].Rows)
                        {
                            var preguntaDTO = new PreguntaDTO
                            {
                                Id = Convert.ToInt32(preguntaRow["id"]),
                                EvaluacionId = Convert.ToInt32(preguntaRow["evaluacionId"]),
                                Texto = preguntaRow["texto"]?.ToString(),
                                Estado = preguntaRow["estado"]?.ToString(),
                                Opciones = new List<OpcionDTO>()
                            };

                            // Parsear las opciones desde JSON
                            string opcionesJson = preguntaRow["opciones"]?.ToString() ?? "[]";

                            _logger.LogInformation($"JSON opciones: {opcionesJson}");

                            try
                            {
                                // Deserializar usando OpcionDTO que coincide con el JSON del SP
                                var opciones = System.Text.Json.JsonSerializer.Deserialize<List<OpcionDTO>>(
                                    opcionesJson,
                                    new System.Text.Json.JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    }
                                );

                                preguntaDTO.Opciones = opciones ?? new List<OpcionDTO>();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error parseando opciones: {ex.Message}");
                                preguntaDTO.Opciones = new List<OpcionDTO>();
                            }

                            evaluacionDTO.Preguntas.Add(preguntaDTO);
                        }
                    }

                    resultado.Data = evaluacionDTO;
                }
                else
                {
                    resultado.Respuesta = "Error";
                    resultado.Leyenda = "No se encontró la evaluación";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetEvaluacionConPreguntas");
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
