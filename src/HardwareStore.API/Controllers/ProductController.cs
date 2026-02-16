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
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "page", "pageSize" })]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProductsForAdmin()
        {
            var products = await _productService.GetAllProductsForAdminAsync();

            var response = new
            {
                data = products,
                totalCount = products.Count()
            };

            return Ok(response);
        }

        // Devuelve producto por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
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

        // Crea nuevo producto (Solo Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto)
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

        // Actualiza producto (Solo Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
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

        // Elimina producto (Solo Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> DeleteProduct(int id)
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

        // Devuelve productos por categoría
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
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

        // Busca productos por término
        [HttpGet("search")]
        [EnableRateLimiting("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
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

        // Devuelve todas las categorías disponibles
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }

        // Devuelve productos por marca
        [HttpGet("brand/{brand}")]
        public async Task<IActionResult> GetProductsByBrand(string brand)
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

        // Devuelve estructura jerárquica
        [HttpGet("menu-structure")]
        public async Task<IActionResult> GetMenuStructure()
        {
            var structure = await _productService.GetProductMenuStructureAsync();
            return Ok(structure);
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

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _productService.GetBrandsAsync();
            return Ok(brands);
        }
    }
}
