using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Helpers;
using EcommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Devuelve todos los productos con paginación
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Valida parámetros de paginación usando SecurityHelper
                var paginationValidation = SecurityHelper.ValidatePaginationParams(page, pageSize);
                if (!paginationValidation.IsValid)
                {
                    Log.Warning("Invalid pagination parameters: Page={Page}, PageSize={PageSize} from IP={IP}",
                        page, pageSize, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = paginationValidation.ErrorMessage });
                }

                var products = await _productService.GetAllProductsAsync();

                // Paginación
                var totalCount = products.Count();
                var paginatedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new
                {
                    data = paginatedProducts,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving products");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve producto por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                // Validar ID usando SecurityHelper
                if (!SecurityHelper.IsValidId(id))
                {
                    Log.Warning("Invalid product ID attempted: {ProductId} from IP: {IP}",
                        id, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    Log.Information("Product not found: {ProductId}", id);
                    return NotFound(new { message = "Producto no encontrado" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving product: {ProductId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Crea nuevo producto (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid product creation attempt by admin: {AdminId}",
                        User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(ModelState);
                }

                // Valida datos del producto usando SecurityHelper
                var validationResult = SecurityHelper.ValidateProductData(
                    createProductDto.Name?.Trim(),
                    createProductDto.Price,
                    createProductDto.Stock,
                    createProductDto.Brand?.Trim(),
                    createProductDto.Model?.Trim()
                );

                if (!validationResult.IsValid)
                {
                    Log.Warning("Product creation validation failed: {Error} by Admin: {AdminId}",
                        validationResult.ErrorMessage, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(new { message = validationResult.ErrorMessage });
                }

                // Valida archivos de imagen si se proporcionan
                if (createProductDto.ImageFiles != null && createProductDto.ImageFiles.Any())
                {
                    var fileValidation = FileValidationHelper.ValidateMultipleProductImages(createProductDto.ImageFiles);
                    if (!fileValidation.IsValid)
                    {
                        Log.Warning("Image validation failed in product creation: {Error}", fileValidation.ErrorMessage);
                        return BadRequest(new { message = fileValidation.ErrorMessage });
                    }
                }

                var product = await _productService.CreateProductAsync(createProductDto);

                Log.Information("Product created successfully: ProductId={ProductId}, Name={Name}, Brand={Brand} by Admin={AdminId}",
                    product.Id, product.Name, product.Brand, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Product creation validation failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Product creation failed by Admin: {AdminId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Actualiza producto (Solo Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
        {
            try
            {
                // Valida ID
                if (!SecurityHelper.IsValidId(id))
                {
                    Log.Warning("Invalid product ID for update: {ProductId} by Admin: {AdminId}",
                        id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                if (!ModelState.IsValid)
                {
                    Log.Warning("Invalid product update attempt: ProductId={ProductId} by Admin={AdminId}",
                        id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(ModelState);
                }

                // Valida datos del producto si se proporcionan
                var validationResult = SecurityHelper.ValidateProductData(
                    updateProductDto.Name?.Trim(),
                    updateProductDto.Price,
                    updateProductDto.Stock,
                    updateProductDto.Brand?.Trim(),
                    updateProductDto.Model?.Trim()
                );

                if (!validationResult.IsValid)
                {
                    Log.Warning("Product update validation failed: {Error} for ProductId={ProductId}",
                        validationResult.ErrorMessage, id);
                    return BadRequest(new { message = validationResult.ErrorMessage });
                }

                // Valida archivos de imagen si se proporcionan
                if (updateProductDto.ImageFiles != null && updateProductDto.ImageFiles.Any())
                {
                    var fileValidation = FileValidationHelper.ValidateMultipleProductImages(updateProductDto.ImageFiles);
                    if (!fileValidation.IsValid)
                    {
                        Log.Warning("Image validation failed in product update: {Error}", fileValidation.ErrorMessage);
                        return BadRequest(new { message = fileValidation.ErrorMessage });
                    }
                }

                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                if (product == null)
                {
                    Log.Warning("Product not found for update: ProductId={ProductId}", id);
                    return NotFound(new { message = "Producto no encontrado" });
                }

                Log.Information("Product updated successfully: ProductId={ProductId}, Name={Name}, Brand={Brand} by Admin={AdminId}",
                    id, product.Name, product.Brand, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Product update validation failed: {Error} for ProductId={ProductId}", ex.Message, id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Product update failed: ProductId={ProductId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Elimina producto (Solo Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // Valida ID
                if (!SecurityHelper.IsValidId(id))
                {
                    Log.Warning("Invalid product ID for deletion: {ProductId} by Admin: {AdminId}",
                        id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    return BadRequest(new { message = "ID de producto inválido" });
                }

                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    Log.Warning("Product not found for deletion: ProductId={ProductId}", id);
                    return NotFound(new { message = "Producto no encontrado" });
                }

                Log.Warning("Product deleted: ProductId={ProductId} by Admin={AdminId}",
                    id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Producto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Product deletion failed: ProductId={ProductId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve productos por categoría
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(new { message = "Categoría requerida" });
                }

                // Sanitiza y valida categoría
                var sanitizedCategory = SecurityHelper.SanitizeCategory(category);
                var limitedCategory = SecurityHelper.LimitStringLength(sanitizedCategory, 50);

                if (string.IsNullOrEmpty(limitedCategory))
                {
                    Log.Warning("Invalid category name: {Category} from IP: {IP}",
                        category, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "Nombre de categoría inválido" });
                }

                var products = await _productService.GetProductsByCategoryAsync(limitedCategory);
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving products by category: {Category}", category);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Busca productos por término
        [HttpGet("search")]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new { message = "El término de búsqueda es requerido" });
                }

                if (term.Length < 2)
                {
                    return BadRequest(new { message = "El término debe tener al menos 2 caracteres" });
                }

                // Sanitiza término de búsqueda
                var sanitizedTerm = SecurityHelper.SanitizeSearchTerm(term);
                var limitedTerm = SecurityHelper.LimitStringLength(sanitizedTerm, 50);

                if (string.IsNullOrEmpty(limitedTerm))
                {
                    Log.Warning("Invalid search term: {Term} from IP: {IP}",
                        term, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "Término de búsqueda inválido" });
                }

                var products = await _productService.SearchProductsAsync(limitedTerm);

                Log.Information("Product search performed: Term={Term}, Results={Count} from IP={IP}",
                    limitedTerm, products.Count(), HttpContext.Connection.RemoteIpAddress);

                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Product search failed: Term={Term}", term);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve todas las categorías disponibles
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving categories");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve productos por marca 
        [HttpGet("brand/{brand}")]
        public async Task<IActionResult> GetProductsByBrand(string brand)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(brand))
                {
                    return BadRequest(new { message = "Marca requerida" });
                }

                var sanitizedBrand = SecurityHelper.SanitizeCategory(brand);
                var limitedBrand = SecurityHelper.LimitStringLength(sanitizedBrand, 50);

                if (string.IsNullOrEmpty(limitedBrand))
                {
                    Log.Warning("Invalid brand name: {Brand} from IP: {IP}",
                        brand, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest(new { message = "Nombre de marca inválido" });
                }

                var products = await _productService.GetProductsByBrandAsync(limitedBrand);
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving products by brand: {Brand}", brand);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Devuelve estructura jerárquica 
        [HttpGet("menu-structure")]
        public async Task<IActionResult> GetMenuStructure()
        {
            try
            {
                var structure = await _productService.GetProductMenuStructureAsync();
                return Ok(structure);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving menu structure");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts(
            [FromQuery] string? category = null,
            [FromQuery] string? brand = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var paginationValidation = SecurityHelper.ValidatePaginationParams(page, pageSize);
                if (!paginationValidation.IsValid)
                {
                    return BadRequest(new { message = paginationValidation.ErrorMessage });
                }

                var sanitizedCategory = string.IsNullOrWhiteSpace(category) ? null :
                    SecurityHelper.LimitStringLength(SecurityHelper.SanitizeCategory(category), 50);

                var sanitizedBrand = string.IsNullOrWhiteSpace(brand) ? null :
                    SecurityHelper.LimitStringLength(SecurityHelper.SanitizeCategory(brand), 50);

                if (minPrice.HasValue && minPrice < 0)
                    return BadRequest(new { message = "El precio mínimo no puede ser negativo" });

                if (maxPrice.HasValue && maxPrice < 0)
                    return BadRequest(new { message = "El precio máximo no puede ser negativo" });

                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                    return BadRequest(new { message = "El precio mínimo no puede ser mayor al máximo" });

                var products = await _productService.FilterProductsAsync(
                    sanitizedCategory, sanitizedBrand, minPrice, maxPrice, inStock);

                var totalCount = products.Count();
                var paginatedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = new
                {
                    data = paginatedProducts,
                    filters = new
                    {
                        category = sanitizedCategory,
                        brand = sanitizedBrand,
                        minPrice,
                        maxPrice,
                        inStock
                    },
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error filtering products");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var brands = await _productService.GetBrandsAsync();
                return Ok(brands);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving brands");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}