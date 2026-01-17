using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AgroPetechBackend.Shared
{
    public class DBXmlMethods
    {
        public static XDocument GetXml<T>(T criterio)
        {
            XDocument resultado = new XDocument(new XDeclaration("1.0", "utf-8", "true"));
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using XmlWriter xw = resultado.CreateWriter();
                xs.Serialize(xw, criterio);
                return resultado;
            }
            catch (Exception ex)
            {
                // Log del error en lugar de solo return null
                Console.WriteLine($"Error en GetXml: {ex.Message}");
                return null;
            }
        }

        public static async Task<DataSet> EjecutaBase(string nombreProcedimiento, string cadenaConexion, string proceso, string dataXML)
        {
            DataSet dsResultado = new DataSet();

            if (string.IsNullOrEmpty(cadenaConexion) || string.IsNullOrEmpty(proceso))
            {
                Console.WriteLine("Cadena de conexión o proceso vacío");
                return dsResultado;
            }

            using (SqlConnection cnn = new SqlConnection(cadenaConexion))
            {
                try
                {
                    await cnn.OpenAsync().ConfigureAwait(false);

                    using (SqlCommand cmd = new SqlCommand(nombreProcedimiento, cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Aumentar timeout para archivos grandes
                        cmd.CommandTimeout = 300; // 5 minutos en lugar de 120 segundos

                        // Usar parámetros optimizados
                        cmd.Parameters.Add("@iTransaccion", SqlDbType.VarChar, 50).Value = proceso;

                        if (!string.IsNullOrEmpty(dataXML))
                        {
                            // Para archivos grandes, usar XmlReader para mejor rendimiento
                            using (XmlReader xmlReader = XmlReader.Create(new StringReader(dataXML)))
                            {
                                cmd.Parameters.Add("@iXml", SqlDbType.Xml).Value = new SqlXml(xmlReader);

                                using (SqlDataAdapter adt = new SqlDataAdapter(cmd))
                                {
                                    adt.Fill(dsResultado);
                                }
                            }
                        }
                        else
                        {
                            cmd.Parameters.Add("@iXml", SqlDbType.Xml).Value = DBNull.Value;

                            using (SqlDataAdapter adt = new SqlDataAdapter(cmd))
                            {
                                adt.Fill(dsResultado);
                            }
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"Error SQL en EjecutaBase: {sqlEx.Message}");
                    Console.WriteLine($"Número de error: {sqlEx.Number}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error general en EjecutaBase: {ex.Message}");
                    throw;
                }
            }
            return dsResultado;
        }

        public static string ConvertToBase64(byte[] data)
        {
            return data != null ? Convert.ToBase64String(data) : string.Empty;
        }

        public static byte[] ConvertFromBase64(string base64)
        {
            return !string.IsNullOrEmpty(base64) ? Convert.FromBase64String(base64) : null;
        }
    }
}