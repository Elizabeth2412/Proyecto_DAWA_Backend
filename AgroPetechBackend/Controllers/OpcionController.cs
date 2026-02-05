using AgroPetechClases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpcionController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OpcionController> _logger;

        public OpcionController(IConfiguration config, ILogger<OpcionController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("GetOpcion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetOpcion([FromBody] Opcion opcion)
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

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(opcion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetOpcion",
                    cadenaConexion,
                    opcion.Transaccion ?? "CONSULTAR_OPCIONES",
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
                    List<Opcion> opciones = new List<Opcion>();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        opciones.Add(new Opcion
                        {
                            Id = Convert.ToInt32(row["id"]),
                            PreguntaId = Convert.ToInt32(row["preguntaId"]),
                            Texto = row["texto"]?.ToString(),
                            EsCorrecta = Convert.ToBoolean(row["esCorrecta"]),
                            Estado = row["estado"]?.ToString()
                        });
                    }

                    resultado.Data = opciones;
                }
                else
                {
                    resultado.Data = new List<Opcion>();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetOpcion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("GetOpcionByPregunta")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetOpcionByPregunta([FromBody] Opcion opcion)
        {
            try
            {
                if (opcion.PreguntaId <= 0)
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "El Id de la pregunta es obligatorio"
                    });

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                opcion.Transaccion = "OPCIONES_POR_PREGUNTA";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(opcion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetOpcion",
                    cadenaConexion,
                    opcion.Transaccion,
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();

                if (dsResultado.Tables.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";
                }

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    List<Opcion> opciones = new List<Opcion>();

                    foreach (DataRow row in dsResultado.Tables[1].Rows)
                    {
                        opciones.Add(new Opcion
                        {
                            Id = Convert.ToInt32(row["id"]),
                            PreguntaId = Convert.ToInt32(row["preguntaId"]),
                            Texto = row["texto"]?.ToString(),
                            EsCorrecta = Convert.ToBoolean(row["esCorrecta"]),
                            Estado = row["estado"]?.ToString()
                        });
                    }

                    resultado.Data = opciones;
                }
                else
                {
                    resultado.Data = new List<Opcion>();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetOpcionByPregunta");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("InsertarOpcion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> InsertarOpcion([FromBody] Opcion opcion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                opcion.Transaccion = "INSERTAR_OPCION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(opcion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetOpcion",
                    cadenaConexion,
                    "INSERTAR_OPCION",
                    xmlParam?.ToString() ?? ""
                );

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en InsertarOpcion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }


        [HttpPost("ActualizarOpcion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarOpcion([FromBody] Opcion opcion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                opcion.Transaccion = "ACTUALIZAR_OPCION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(opcion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetOpcion",
                    cadenaConexion,
                    "ACTUALIZAR_OPCION",
                    xmlParam?.ToString() ?? ""
                );

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarOpcion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpPost("EliminarOpcion")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarOpcion([FromBody] Opcion opcion)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                opcion.Transaccion = "ELIMINAR_OPCION";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(opcion);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetOpcion",
                    cadenaConexion,
                    "ELIMINAR_OPCION",
                    xmlParam?.ToString() ?? ""
                );

                var resultado = new Resultado
                {
                    Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString(),
                    Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString()
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarOpcion");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }

    }
}
