namespace EcommerceAPI.Helpers
{
    public static class FileValidationHelper
    {
        public static (bool IsValid, string ErrorMessage) ValidatePaymentReceipt(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "No se proporcionó archivo de comprobante");

            // Tamaño máximo: 5MB
            if (file.Length > 5 * 1024 * 1024)
                return (false, "El archivo no puede exceder 5MB");

            // Extensiones permitidas
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                return (false, "Solo se permiten archivos JPG, PNG o PDF");

            // MIME types permitidos
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "Tipo de contenido no válido");

            // Validar magic numbers (primeros bytes del archivo)
            return ValidateFileSignature(file);
        }

        public static (bool IsValid, string ErrorMessage) ValidateProductImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "Archivo vacío");

            // Tamaño máximo por imagen: 2MB
            if (file.Length > 2 * 1024 * 1024)
                return (false, "La imagen no puede exceder 2MB");

            // Solo imágenes
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                return (false, "Solo se permiten archivos JPG, PNG o WebP");

            // MIME types para imágenes
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "Tipo de contenido no válido");

            // Validar magic numbers
            return ValidateFileSignature(file);
        }

        public static (bool IsValid, string ErrorMessage) ValidateMultipleProductImages(IFormFile[] files, int maxCount = 10)
        {
            if (files == null || !files.Any())
                return (true, "");

            if (files.Length > maxCount)
                return (false, $"Máximo {maxCount} archivos permitidos");

            // Valida cada archivo individualmente
            foreach (var file in files)
            {
                var validation = ValidateProductImage(file);
                if (!validation.IsValid)
                    return (false, $"Archivo '{file.FileName}': {validation.ErrorMessage}");
            }

            return (true, "");
        }

        private static (bool IsValid, string ErrorMessage) ValidateFileSignature(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[8]; // Lee más bytes para WebP
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead < 4)
                    return (false, "Archivo demasiado pequeño para validar");

                // Reset para no afectar lecturas posteriores
                stream.Position = 0;

                // Validar según tipo de contenido
                if (file.ContentType.Contains("jpeg") || file.ContentType.Contains("jpg"))
                {
                    // JPEG: FF D8
                    if (!(buffer[0] == 0xFF && buffer[1] == 0xD8))
                        return (false, "Archivo JPEG inválido");
                }
                else if (file.ContentType.Contains("png"))
                {
                    // PNG: 89 50 4E 47
                    if (!(buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47))
                        return (false, "Archivo PNG inválido");
                }
                else if (file.ContentType.Contains("pdf"))
                {
                    // PDF: 25 50 44 46
                    if (!(buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46))
                        return (false, "Archivo PDF inválido");
                }
                else if (file.ContentType.Contains("webp"))
                {
                    // WebP: RIFF en posición 0, WEBP en posición 8
                    if (bytesRead >= 8)
                    {
                        var riffSignature = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46;
                        // Necesita leer más para validar WEBP signature
                        if (!riffSignature)
                            return (false, "Archivo WebP inválido");
                    }
                    else
                    {
                        return (false, "Archivo WebP incompleto");
                    }
                }

                return (true, "");
            }
            catch (Exception)
            {
                return (false, "No se pudo validar el archivo");
            }
        }

        public static (bool IsValid, string ErrorMessage) ValidateTotalFileSize(IFormFile[] files, long maxTotalSizeBytes)
        {
            if (files == null || !files.Any())
                return (true, "");

            var totalSize = files.Sum(f => f?.Length ?? 0);

            if (totalSize > maxTotalSizeBytes)
            {
                var maxSizeMB = maxTotalSizeBytes / (1024 * 1024);
                var actualSizeMB = totalSize / (1024 * 1024);
                return (false, $"Tamaño total excede el límite. Límite: {maxSizeMB}MB, Actual: {actualSizeMB}MB");
            }

            return (true, "");
        }
    }
}