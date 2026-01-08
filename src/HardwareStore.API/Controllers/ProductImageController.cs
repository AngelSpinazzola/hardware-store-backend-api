using HardwareStore.Application.Products;
using HardwareStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace HardwareStore.API.Controllers
{
    [ApiController]
    [Route("api/product/{productId}/images")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                // Valida ID usando SecurityHelper
                if (!SecurityHelper.IsValidId(productId))
                {
                    Log.Warning("Invalid productId for images: {ProductId} from IP: {IP}",
                        productId, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                var images = await _productImageService.GetProductImagesAsync(productId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving product images: ProductId={ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{imageId}")]
        public async Task<IActionResult> GetProductImage(int productId, int imageId)
        {
            try
            {
                // Valida IDs usando SecurityHelper
                if (!SecurityHelper.AreValidIds(productId, imageId))
                {
                    Log.Warning("Invalid IDs for product image: ProductId={ProductId}, ImageId={ImageId} from IP={IP}",
                        productId, imageId, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "IDs inválidos" });
                }

                var image = await _productImageService.GetProductImageAsync(productId, imageId);
                if (image == null)
                {
                    Log.Information("Product image not found: ProductId={ProductId}, ImageId={ImageId}",
                        productId, imageId);
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                return Ok(image);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving product image: ProductId={ProductId}, ImageId={ImageId}",
                    productId, imageId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> CreateProductImages(int productId, [FromForm] CreateProductImageDto createDto)
        {
            try
            {
                // Valida ID
                if (!SecurityHelper.IsValidId(productId))
                {
                    var adminInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid productId for image creation: {ProductId} by Admin: {AdminId}",
                        productId, adminInfo.UserId);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                if (!ModelState.IsValid)
                {
                    var adminValidationInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid image creation attempt: ProductId={ProductId} by Admin={AdminId}",
                        productId, adminValidationInfo.UserId);
                    return BadRequest(ModelState);
                }

                // Valida archivos usando FileValidationHelper
                if (createDto.ImageFiles == null || !createDto.ImageFiles.Any())
                {
                    return BadRequest(new { message = "Se requiere al menos una imagen" });
                }

                // Valida cantidad de archivos
                if (createDto.ImageFiles.Length > 10)
                {
                    Log.Warning("Too many images uploaded: {Count} for ProductId={ProductId}",
                        createDto.ImageFiles.Length, productId);
                    return BadRequest(new { message = "Máximo 10 imágenes por producto" });
                }

                // Valida cada archivo individualmente
                var fileValidation = FileValidationHelper.ValidateMultipleProductImages(createDto.ImageFiles);
                if (!fileValidation.IsValid)
                {
                    Log.Warning("Image validation failed: {Error} for ProductId={ProductId}",
                        fileValidation.ErrorMessage, productId);
                    return BadRequest(new { message = fileValidation.ErrorMessage });
                }

                // Valida tamaño total
                var totalSizeValidation = FileValidationHelper.ValidateTotalFileSize(
                    createDto.ImageFiles, 10 * 1024 * 1024); // 10MB total
                if (!totalSizeValidation.IsValid)
                {
                    Log.Warning("Total file size validation failed: {Error} for ProductId={ProductId}",
                        totalSizeValidation.ErrorMessage, productId);
                    return BadRequest(new { message = totalSizeValidation.ErrorMessage });
                }

                createDto.ProductId = productId;
                var images = await _productImageService.CreateProductImagesAsync(createDto);

                var adminSuccessInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Information("Product images created: ProductId={ProductId}, Count={Count} by Admin={AdminId}",
                    productId, images.Count(), adminSuccessInfo.UserId);

                return Ok(new { message = "Imágenes agregadas correctamente", images });
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Image creation validation failed: {Error} for ProductId={ProductId}",
                    ex.Message, productId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Image creation failed: ProductId={ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{imageId}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            try
            {
                // Valida IDs
                if (!SecurityHelper.AreValidIds(productId, imageId))
                {
                    var adminDeleteInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid IDs for image deletion: ProductId={ProductId}, ImageId={ImageId} by Admin={AdminId}",
                        productId, imageId, adminDeleteInfo.UserId);
                    return BadRequest(new { message = "IDs inválidos" });
                }

                var result = await _productImageService.DeleteProductImageAsync(productId, imageId);
                if (!result)
                {
                    Log.Warning("Image not found for deletion: ProductId={ProductId}, ImageId={ImageId}",
                        productId, imageId);
                    return NotFound(new { message = "Imagen no encontrada" });
                }

                var adminDeleteSuccessInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Information("Product image deleted: ProductId={ProductId}, ImageId={ImageId} by Admin={AdminId}",
                    productId, imageId, adminDeleteSuccessInfo.UserId);

                return Ok(new { message = "Imagen eliminada correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Image deletion failed: ProductId={ProductId}, ImageId={ImageId}",
                    productId, imageId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{imageId}/main")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            try
            {
                // Valida IDs
                if (!SecurityHelper.AreValidIds(productId, imageId))
                {
                    var adminMainInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid IDs for main image setting: ProductId={ProductId}, ImageId={ImageId} by Admin={AdminId}",
                        productId, imageId, adminMainInfo.UserId);
                    return BadRequest(new { message = "IDs inválidos" });
                }

                var result = await _productImageService.SetMainImageAsync(productId, imageId);
                if (!result)
                {
                    Log.Warning("Could not set main image: ProductId={ProductId}, ImageId={ImageId}",
                        productId, imageId);
                    return BadRequest(new { message = "No se pudo establecer como imagen principal" });
                }

                var adminMainSuccessInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Information("Main image set: ProductId={ProductId}, ImageId={ImageId} by Admin={AdminId}",
                    productId, imageId, adminMainSuccessInfo.UserId);

                return Ok(new { message = "Imagen principal actualizada" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Set main image failed: ProductId={ProductId}, ImageId={ImageId}",
                    productId, imageId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("order")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateImagesOrder(int productId, [FromBody] UpdateProductImagesOrderDto updateOrderDto)
        {
            try
            {
                // Valida ID
                if (!SecurityHelper.IsValidId(productId))
                {
                    var adminOrderInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid productId for order update: {ProductId} by Admin: {AdminId}",
                        productId, adminOrderInfo.UserId);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                if (!ModelState.IsValid)
                {
                    var adminOrderValidationInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid order update attempt: ProductId={ProductId} by Admin={AdminId}",
                        productId, adminOrderValidationInfo.UserId);
                    return BadRequest(ModelState);
                }

                var result = await _productImageService.UpdateImagesOrderAsync(productId, updateOrderDto);
                if (!result)
                {
                    Log.Warning("Could not update images order: ProductId={ProductId}", productId);
                    return BadRequest(new { message = "No se pudo actualizar el orden de las imágenes" });
                }

                var adminOrderSuccessInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Information("Images order updated: ProductId={ProductId} by Admin={AdminId}",
                    productId, adminOrderSuccessInfo.UserId);

                return Ok(new { message = "Orden de imágenes actualizado" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Update images order failed: ProductId={ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("upload-multiple")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UploadMultipleImages(
            int productId,
            [FromForm] IFormFile[] files,
            [FromForm] int? mainImageIndex = null)
        {
            try
            {
                // Valida ID
                if (!SecurityHelper.IsValidId(productId))
                {
                    var adminUploadInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                    Log.Warning("Invalid productId for multiple upload: {ProductId} by Admin: {AdminId}",
                        productId, adminUploadInfo.UserId);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                if (files == null || !files.Any())
                {
                    return BadRequest(new { message = "No se proporcionaron archivos" });
                }

                if (files.Length > 10)
                {
                    Log.Warning("Too many files in multiple upload: {Count} for ProductId={ProductId}",
                        files.Length, productId);
                    return BadRequest(new { message = "Máximo 10 archivos por vez" });
                }

                // Valida todos los archivos usando helper
                var fileValidation = FileValidationHelper.ValidateMultipleProductImages(files);
                if (!fileValidation.IsValid)
                {
                    Log.Warning("File validation failed in multiple upload: {Error} for ProductId={ProductId}",
                        fileValidation.ErrorMessage, productId);
                    return BadRequest(new { message = fileValidation.ErrorMessage });
                }

                // Valida tamaño total
                var totalSizeValidation = FileValidationHelper.ValidateTotalFileSize(files, 15 * 1024 * 1024);
                if (!totalSizeValidation.IsValid)
                {
                    Log.Warning("Total size validation failed: {Error} for ProductId={ProductId}",
                        totalSizeValidation.ErrorMessage, productId);
                    return BadRequest(new { message = totalSizeValidation.ErrorMessage });
                }

                var createImageDto = new CreateProductImageDto
                {
                    ProductId = productId,
                    ImageFiles = files,
                    MainImageIndex = mainImageIndex ?? 0
                };

                // Elimina imágenes existentes
                var existingImages = await _productImageService.GetProductImagesAsync(productId);
                foreach (var existingImage in existingImages)
                {
                    await _productImageService.DeleteProductImageAsync(productId, existingImage.Id);
                }

                var uploadedImages = await _productImageService.CreateProductImagesAsync(createImageDto);

                var adminUploadSuccessInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Information("Multiple images uploaded: ProductId={ProductId}, Count={Count} by Admin={AdminId}",
                    productId, uploadedImages.Count(), adminUploadSuccessInfo.UserId);

                return Ok(uploadedImages);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Multiple upload validation failed: {Error} for ProductId={ProductId}",
                    ex.Message, productId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Multiple upload failed: ProductId={ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}