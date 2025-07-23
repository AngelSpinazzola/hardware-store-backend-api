using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EcommerceAPI.Services
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf" }; 
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileService(IConfiguration configuration, ILogger<FileService> logger)
        {
            _logger = logger;

            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string folder = "products")
        {
            Console.WriteLine($"🔍 SaveImageAsync called with: {imageFile.FileName}");

            try
            {
                if (folder == "payment-receipts" || folder == "receipts")
                {
                    if (!IsValidReceiptFile(imageFile))
                    {
                        throw new ArgumentException("Archivo no válido. Solo se permiten imágenes (JPG, PNG) o PDF");
                    }
                }
                else
                {
                    if (!IsValidImageFile(imageFile))
                    {
                        throw new ArgumentException("Archivo de imagen no válido");
                    }
                }

                using var stream = imageFile.OpenReadStream();

                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (extension == ".pdf")
                {
                    var uploadParams = new RawUploadParams()
                    {
                        File = new FileDescription(imageFile.FileName, stream),
                        Folder = folder,
                        UseFilename = false,
                        UniqueFilename = true,
                        Overwrite = false
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogError($"Error al subir PDF a Cloudinary: {uploadResult.Error.Message}");
                        throw new Exception($"Error al subir archivo: {uploadResult.Error.Message}");
                    }

                    _logger.LogInformation($"PDF subido exitosamente: {uploadResult.SecureUrl}");
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    // Upload como imagen para JPG, PNG, etc.
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(imageFile.FileName, stream),
                        Folder = folder,
                        UseFilename = false,
                        UniqueFilename = true,
                        Overwrite = false,
                        Transformation = new Transformation()
                            .Quality("auto")
                            .FetchFormat("auto")
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                    {
                        _logger.LogError($"Error al subir imagen a Cloudinary: {uploadResult.Error.Message}");
                        throw new Exception($"Error al subir imagen: {uploadResult.Error.Message}");
                    }

                    _logger.LogInformation($"Imagen subida exitosamente: {uploadResult.SecureUrl}");
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SaveImageAsync");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                if (!imagePath.Contains("cloudinary.com"))
                    return true;

                var publicId = ExtractPublicIdFromUrl(imagePath);
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                _logger.LogInformation($"Archivo eliminado: {publicId}, Resultado: {result.Result}");
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar archivo: {imagePath}");
                return false;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
                return false;

            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            return allowedMimeTypes.Contains(file.ContentType.ToLower());
        }

        // Método para validar archivos de comprobantes (imágenes + PDF)
        public bool IsValidReceiptFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allAllowedExtensions = _allowedImageExtensions.Concat(_allowedDocumentExtensions).ToArray();

            if (!allAllowedExtensions.Contains(extension))
                return false;

            // Validar MIME types para imágenes y PDFs
            var allowedMimeTypes = new[] {
                "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp",
                "application/pdf"
            };

            return allowedMimeTypes.Contains(file.ContentType.ToLower());
        }

        public async Task<List<string>> SaveMultipleImagesAsync(IFormFile[] imageFiles, string folder = "products")
        {
            var uploadedUrls = new List<string>();

            foreach (var file in imageFiles)
            {
                try
                {
                    var url = await SaveImageAsync(file, folder);
                    uploadedUrls.Add(url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al subir archivo: {file.FileName}");
                }
            }

            return uploadedUrls;
        }

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var pathParts = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(pathParts, "upload");
                if (uploadIndex >= 0 && uploadIndex + 2 < pathParts.Length)
                {
                    var publicIdParts = pathParts.Skip(uploadIndex + 2).ToArray();
                    var publicId = string.Join("/", publicIdParts);

                    var lastDotIndex = publicId.LastIndexOf('.');
                    if (lastDotIndex > 0)
                        publicId = publicId.Substring(0, lastDotIndex);

                    return publicId;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }


    }
}