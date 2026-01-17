using AgroPetechBackend.DTOs;
using AgroPetechClases;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AgroPetechBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivoController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ArchivoController> _logger;

        public ArchivoController(IConfiguration config, ILogger<ArchivoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        /* Lo que hace este método en el swagger:
         * 1: Para subida de archivos. NOTA: Usar el tipo de usuario: instructor
        - Content-Type: multipart/form-data
        - archivo: Selecciona un archivo (PDF, PPTX menor a 50MB)
        - descripcion: "Archivo de prueba"
        - usuario: "leslie@gmail.com"

         * 2. Buscar archivo por usuario:
         {
            "usuario": "leslie@gmail.com",
            "transaccion": "ARCHIVOS_POR_USUARIO"
        }

         */
        [HttpPost("SetArchivo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> SetArchivo(
    [FromForm] ArchivoFormRequest request)
        {
            try
            {
                var archivo = request.Archivo;
                var descripcion = request.Descripcion;
                var usuario = request.Usuario;

                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Archivo requerido" });

                // Validar tipo de archivo
                var extension = Path.GetExtension(archivo.FileName).ToLower();
                if (!new[] { ".pdf", ".pptx" }.Contains(extension))
                {
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = "Tipo de archivo no permitido. Solo se aceptan: PDF, PPTX"
                    });
                }

                // Limitar tamaño máximo (ejemplo: 50MB)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (archivo.Length > maxFileSize)
                {
                    return BadRequest(new Resultado
                    {
                        Respuesta = "Error",
                        Leyenda = $"El archivo es demasiado grande. Tamaño máximo: {maxFileSize / (1024 * 1024)}MB"
                    });
                }

                // Leer el archivo en chunks para evitar memory pressure
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await archivo.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Convertir a Base64 para enviar al SQL
                string base64Content = Convert.ToBase64String(fileBytes);

                // Crear objeto Archivo con los datos
                var archivoObj = new Archivo
                {
                    Nombre = Path.GetFileNameWithoutExtension(archivo.FileName),
                    Tipo = extension.Replace(".", "").ToUpper(),
                    Tamano = archivo.Length,
                    Contenido = base64Content,
                    Descripcion = descripcion ?? $"Archivo {Path.GetFileNameWithoutExtension(archivo.FileName)}",
                    Usuario = usuario,
                    Estado = "Disponible",
                    Transaccion = "INSERTAR_ARCHIVO"
                };

                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");

                // Agregar log para depuración
                _logger.LogInformation($"Subiendo archivo: {archivo.FileName}, Tamaño: {archivo.Length} bytes, Usuario: {usuario}");

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(archivoObj);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetArchivo",
                    cadenaConexion,
                    "INSERTAR_ARCHIVO",
                    xmlParam?.ToString() ?? "");

                var resultado = new Resultado();
                if (dsResultado.Tables.Count > 0 && dsResultado.Tables[0].Rows.Count > 0)
                {
                    resultado.Respuesta = dsResultado.Tables[0].Rows[0]["Respuesta"]?.ToString() ?? "Error";
                    resultado.Leyenda = dsResultado.Tables[0].Rows[0]["Leyenda"]?.ToString() ?? "Sin mensaje";

                    // Agregar log del resultado
                    _logger.LogInformation($"Resultado de subida: {resultado.Respuesta} - {resultado.Leyenda}");
                }
                else
                {
                    resultado.Respuesta = "Error";
                    resultado.Leyenda = "No se recibió respuesta del servidor";
                    _logger.LogWarning("No se recibió respuesta del servidor para la subida de archivo");
                }

                return Ok(resultado);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Error de SQL en SetArchivo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error de base de datos: {sqlEx.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SetArchivo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /* Lo que hace este método en el swagger:
         * 1: Consulta de archivos
        {
            "transaccion": "CONSULTAR_ARCHIVO"
        }
         */
        [HttpPost("GetArchivo")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> GetArchivo([FromBody] Archivo archivo)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(archivo);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetArchivo",
                    cadenaConexion,
                    archivo.Transaccion ?? "CONSULTAR_ARCHIVO",
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

                    // Tabla 1: Datos de archivos
                    if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                    {
                        List<Archivo> archivos = new List<Archivo>();
                        foreach (DataRow row in dsResultado.Tables[1].Rows)
                        {
                            try
                            {
                                var file = new Archivo
                                {
                                    Id = Convert.ToInt32(row["id"]),
                                    Nombre = row["nombre"]?.ToString(),
                                    Tipo = row["tipo"]?.ToString(),
                                    Descripcion = row["descripcion"]?.ToString(),
                                    Usuario = row["usuario"]?.ToString(),
                                    Estado = row["estado"]?.ToString(),
                                    FechaSubida = Convert.ToDateTime(row["fechaSubida"])
                                };

                                if (row["tamano"] != DBNull.Value && row["tamano"] != null)
                                {
                                    file.Tamano = Convert.ToInt64(row["tamano"]);
                                }

                                archivos.Add(file);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error procesando fila: {ex.Message}");
                            }
                        }
                        resultado.Data = archivos;
                    }
                    else
                    {
                        resultado.Data = new List<Archivo>();
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
                _logger.LogError(ex, "Error en GetArchivo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /*  Lo que hace este método en el swagger:
         * 1: Actualizar datos de archivo:
         {
          "id": 1,
          "nombre": "Archivo Actualizado",
          "descripcion": "Descripción actualizada",
          "estado": "Disponible"
        }
         */
        [HttpPost("ActualizarArchivo")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> ActualizarArchivo([FromBody] Archivo archivo)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                archivo.Transaccion = "ACTUALIZAR_ARCHIVO";

                XDocument xmlParam = Shared.DBXmlMethods.GetXml(archivo);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetArchivo",
                    cadenaConexion,
                    "ACTUALIZAR_ARCHIVO",
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
                _logger.LogError(ex, "Error en ActualizarArchivo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
        /* Lo que hace este método en el swagger:
         * 1: Descargar archivo por id
        Ejemplo: GET /api/Archivo/DescargarArchivo/1
         */
        [HttpGet("DescargarArchivo/{id}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DescargarArchivo(int id)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest("Cadena de conexión no configurada");

                var archivo = new Archivo { Id = id, Transaccion = "BUSCAR_ARCHIVO" };
                XDocument xmlParam = Shared.DBXmlMethods.GetXml(archivo);

                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "GetArchivo",
                    cadenaConexion,
                    "BUSCAR_ARCHIVO",
                    xmlParam?.ToString() ?? ""
                );

                if (dsResultado.Tables.Count > 1 && dsResultado.Tables[1].Rows.Count > 0)
                {
                    var row = dsResultado.Tables[1].Rows[0];

                    //  Contenido viene como BYTE[] (Base64)
                    byte[] contenidoBytes = row["contenido"] as byte[];
                    if (contenidoBytes == null || contenidoBytes.Length == 0)
                        return NotFound("Archivo sin contenido");

                    //  Convertir Base64 (guardado como texto) a bytes reales
                    string base64Content = Encoding.UTF8.GetString(contenidoBytes);
                    byte[] fileBytes = Convert.FromBase64String(base64Content);

                    string nombre = row["nombre"]?.ToString() ?? "archivo";
                    string tipo = row["tipo"]?.ToString() ?? "PDF";

                    string fileName = nombre + GetExtension(tipo);

                    return File(fileBytes, GetMimeType(tipo), fileName);
                }

                return NotFound("Archivo no encontrado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DescargarArchivo");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private string GetMimeType(string tipo)
        {
            return tipo.ToUpper() switch
            {
                "PDF" => "application/pdf",
                "PPTX" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
               
                _ => "application/octet-stream"
            };
        }

        private string GetExtension(string tipo)
        {
            return tipo.ToUpper() switch
            {
                "PDF" => ".pdf",
                "PPTX" => ".pptx",
                _ => ""
            };
        }
        /* Lo que hace este método en el swagger:
         * 1: Descargar archivo por id
        Ejemplo: DELETE /api/Archivo/EliminarArchivo/1
         */

        [HttpDelete("EliminarArchivo/{id}")]
        [ProducesResponseType(typeof(Resultado), 200)]
        [ProducesResponseType(typeof(Resultado), 400)]
        [ProducesResponseType(typeof(Resultado), 500)]
        public async Task<ActionResult<Resultado>> EliminarArchivo(int id)
        {
            try
            {
                var cadenaConexion = _config.GetConnectionString("AgroPetechConnection");
                if (string.IsNullOrEmpty(cadenaConexion))
                    return BadRequest(new Resultado { Respuesta = "Error", Leyenda = "Cadena de conexión no configurada" });

                var archivo = new Archivo { Id = id, Transaccion = "ELIMINAR_ARCHIVO" };
                XDocument xmlParam = Shared.DBXmlMethods.GetXml(archivo);
                DataSet dsResultado = await Shared.DBXmlMethods.EjecutaBase(
                    "SetArchivo",
                    cadenaConexion,
                    "ELIMINAR_ARCHIVO",
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
                _logger.LogError(ex, "Error en EliminarArchivo");
                return StatusCode(500, new Resultado
                {
                    Respuesta = "Error",
                    Leyenda = $"Error interno: {ex.Message}"
                });
            }
        }
    }


}