using AgroPetechClases;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IConfiguration config, ILogger<UsuarioController> logger)
        {
            _config = config;
            _logger = logger;
        }
        /* Lo que hace este método en el swagger:
         * 1: Listar todos los usuarios
        {
          "transaccion": "CONSULTAR_USUARIO"
        }
         */
        [HttpPost("GetUsuario")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetUsuario",
                    cadenaConexion,
                    usuario.Transaccion ?? "CONSULTAR_USUARIO",
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

                    // Tabla 1: Datos de usuarios
                    if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                    {
                        List<Usuario> usuarios = new List<Usuario>();
                        foreach (DataRow row in dsResultado.Tables[1].Rows)
                        {
                            try
                            {
                                var user = new Usuario
                                {
                                    Id = Convert.ToInt32(row["id"]),
                                    Email = row["email"]?.ToString(),
                                    Tipo = row["tipo"]?.ToString(),
                                    Nombre = row["nombre"]?.ToString(),
                                    Apellido = row["apellido"]?.ToString()
                                };

                                if (row["edad"] != DBNull.Value && row["edad"] != null)
                                {
                                    user.Edad = Convert.ToInt32(row["edad"]);
                                }

                                usuarios.Add(user);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error procesando fila: {ex.Message}");
                            }
                        }
                        resultado.Data = usuarios;
                    }
                    else
                    {
                        resultado.Data = new List<Usuario>();
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
                _logger.LogError(ex, "Error en GetUsuario");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /* Lo que hace este método en el swagger:
         * 1: Actualizar usuario
        {
        "email": "leslie@gmail.com",
        "nombre": "Leslie Actualizado",
        "apellido": "Vera Actualizado",
        "edad": 21,
        "tipo": "instructor",
        "transaccion": "ACTUALIZAR_USUARIO"
        }
        * 2: Registrar un usuario
        {
          "email": "nuevo@email.com",
          "password": "123456",
          "tipo": "estudiante",
          "nombre": "Nuevo",
          "apellido": "Usuario",
          "edad": 25
        }
        * 3: Eliminar usuario por id
        {
          "email": "nuevo@email.com",
        "transaccion": "ELIMINAR_USUARIO"
        }
         */
        [HttpPost("SetUsuario")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> SetUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(usuario);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetUsuario",
                    cadenaConexion,
                    usuario.Transaccion ?? "INSERTAR_USUARIO",
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
                _logger.LogError(ex, "Error en SetUsuario");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /* Lo que hace este método en el swagger:
         * 1: Validación de credenciales
        {
          "email": "leslie@gmail.com",
          "password": "instructor123"
        }

         */
        [HttpPost("ValidarLogin")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ValidarLogin([FromBody] Usuario usuario)
        {
            try
            {
                usuario.Transaccion = "VALIDAR_USUARIO";
                return await GetUsuario(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ValidarLogin");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /* Lo que hace este método en el swagger:
         * 1: Registrar un usuario
        {
          "email": "nuevo@email.com",
          "password": "123456",
          "tipo": "estudiante",
          "nombre": "Nuevo",
          "apellido": "Usuario",
          "edad": 25
        }
         */
        [HttpPost("RegistrarUsuario")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> RegistrarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                usuario.Transaccion = "INSERTAR_USUARIO";
                return await SetUsuario(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RegistrarUsuario");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
    }
}