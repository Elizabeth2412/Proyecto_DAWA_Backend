using Minio;
using Minio.DataModel.Args;

namespace AgroPetechBackend.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minio;
        private readonly IConfiguration _config;
        private readonly ILogger<MinioService> _logger;

        public MinioService(
            IMinioClient minio,
            IConfiguration config,
            ILogger<MinioService> logger)
        {
            _minio = minio;
            _config = config;
            _logger = logger;
        }

        public async Task<string> SubirArchivoAsync(
            IFormFile archivo,
            string carpeta)
        {
            try
            {
                // Validaciones
                if (archivo == null || archivo.Length == 0)
                    throw new ArgumentException("Archivo vacío o nulo");

                var bucket = _config["Minio:Bucket"];
                var publicUrl = _config["Minio:PublicUrl"];

                // Verificar si el bucket existe
                var bucketExists = await _minio.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucket));

                if (!bucketExists)
                {
                    _logger.LogWarning($"Bucket {bucket} no existe, creándolo...");
                    await _minio.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(bucket));
                }

                // Generar nombre único
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var objectName = $"{carpeta}/{timestamp}-{Guid.NewGuid()}{extension}";

                // Subir archivo
                using var stream = archivo.OpenReadStream();
                var args = new PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(archivo.Length)
                    .WithContentType(archivo.ContentType);

                await _minio.PutObjectAsync(args);

                // Retornar URL pública
                var url = $"{publicUrl}/{bucket}/{objectName}";
                _logger.LogInformation($"Archivo subido: {url}");

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo a MinIO");
                throw;
            }
        }

        public async Task<bool> EliminarArchivoAsync(string objectName)
        {
            try
            {
                var bucket = _config["Minio:Bucket"];
                await _minio.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(objectName));

                _logger.LogInformation($"Archivo eliminado: {objectName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo de MinIO");
                return false;
            }
        }
    }
}
