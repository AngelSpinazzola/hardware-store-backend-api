using EcommerceAPI.DTOs;
using EcommerceAPI.DTOs.Products;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories.Interfaces;
using EcommerceAPI.Services.Interfaces;
using Serilog;

namespace EcommerceAPI.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IFileService _fileService;
        private const string DEFAULT_PLACEHOLDER = "https://placehold.co/600x600/e5e7eb/6b7280/png?text=Sin+Imagen";

        public ProductService(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository,
            IFileService fileService)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _fileService = fileService;
        }

        public async Task<IEnumerable<ProductListDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                Platform = p.Platform,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<IEnumerable<ProductListDto>> GetAllProductsForAdminAsync()
        {
            var products = await _productRepository.GetAllForAdminAsync(); 
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                Platform = p.Platform,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            var images = await _productImageRepository.GetByProductIdAsync(id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                Brand = product.Brand,
                Model = product.Model,
                Platform = product.Platform,
                MainImageUrl = product.MainImageUrl,
                Status = product.Status,      
                DeletedAt = product.DeletedAt,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    IsMain = img.IsMain,
                    CreatedAt = img.CreatedAt
                }).ToList()
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name.Trim(),
                Description = createProductDto.Description?.Trim() ?? string.Empty,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock,
                Category = createProductDto.Category.Trim(),
                Brand = createProductDto.Brand.Trim(),
                Model = createProductDto.Model?.Trim() ?? string.Empty,
                Platform = createProductDto.Platform?.Trim(),
                MainImageUrl = DEFAULT_PLACEHOLDER,
                Status = ProductStatus.Active
            };

            var createdProduct = await _productRepository.CreateAsync(product);

            string mainImageUrl = DEFAULT_PLACEHOLDER;
            var productImages = new List<ProductImage>();

            if (createProductDto.ImageFiles != null && createProductDto.ImageFiles.Length > 0)
            {
                for (int i = 0; i < createProductDto.ImageFiles.Length; i++)
                {
                    var imageFile = createProductDto.ImageFiles[i];
                    var imageUrl = await _fileService.SaveImageAsync(imageFile);

                    var productImage = new ProductImage
                    {
                        ProductId = createdProduct.Id,
                        ImageUrl = imageUrl,
                        DisplayOrder = i,
                        IsMain = i == 0
                    };

                    var createdImage = await _productImageRepository.CreateAsync(productImage);
                    productImages.Add(createdImage);

                    if (i == 0) mainImageUrl = imageUrl;
                }
            }

            if (createProductDto.ImageUrls != null && createProductDto.ImageUrls.Length > 0)
            {
                int startOrder = createProductDto.ImageFiles?.Length ?? 0;
                for (int i = 0; i < createProductDto.ImageUrls.Length; i++)
                {
                    var imageUrl = createProductDto.ImageUrls[i];
                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        var productImage = new ProductImage
                        {
                            ProductId = createdProduct.Id,
                            ImageUrl = imageUrl,
                            DisplayOrder = startOrder + i,
                            IsMain = startOrder == 0 && i == 0
                        };

                        var createdImage = await _productImageRepository.CreateAsync(productImage);
                        productImages.Add(createdImage);

                        if (startOrder == 0 && i == 0) mainImageUrl = imageUrl;
                    }
                }
            }

            createdProduct.MainImageUrl = mainImageUrl;
            await _productRepository.UpdateAsync(createdProduct.Id, createdProduct);

            Log.Information("Product created: ProductId={ProductId}, Name={Name}, Brand={Brand}, HasImages={HasImages}",
                createdProduct.Id, createdProduct.Name, createdProduct.Brand, productImages.Any());

            return new ProductDto
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Description = createdProduct.Description,
                Price = createdProduct.Price,
                Stock = createdProduct.Stock,
                Category = createdProduct.Category,
                Brand = createdProduct.Brand,
                Model = createdProduct.Model,
                MainImageUrl = createdProduct.MainImageUrl,
                Status = createdProduct.Status,       
                DeletedAt = createdProduct.DeletedAt,
                CreatedAt = createdProduct.CreatedAt,
                UpdatedAt = createdProduct.UpdatedAt,
                Images = productImages.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    IsMain = img.IsMain,
                    CreatedAt = img.CreatedAt
                }).ToList()
            };
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                return null;

            var product = new Product
            {
                Id = id,
                Name = updateProductDto.Name?.Trim() ?? existingProduct.Name,
                Description = string.IsNullOrWhiteSpace(updateProductDto.Description)
                    ? null
                    : updateProductDto.Description.Trim(),
                Price = updateProductDto.Price ?? existingProduct.Price,
                Stock = updateProductDto.Stock ?? existingProduct.Stock,
                Category = updateProductDto.Category?.Trim() ?? existingProduct.Category,
                Brand = updateProductDto.Brand?.Trim() ?? existingProduct.Brand,
                Model = updateProductDto.Model?.Trim() ?? existingProduct.Model,
                Platform = updateProductDto.Platform?.Trim() ?? existingProduct.Platform,
                Status = updateProductDto.Status ?? existingProduct.Status,
                MainImageUrl = existingProduct.MainImageUrl,
                CreatedAt = existingProduct.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            // Obtener imágenes existentes antes de procesar nuevas
            var existingImages = await _productImageRepository.GetByProductIdAsync(id);

            // Procesar nuevas imágenes si las hay
            if (updateProductDto.ImageFiles != null && updateProductDto.ImageFiles.Length > 0)
            {
                int startOrder = existingImages.Any() ? existingImages.Max(img => img.DisplayOrder) + 1 : 0;
                bool shouldSetAsMain = !existingImages.Any();

                for (int i = 0; i < updateProductDto.ImageFiles.Length; i++)
                {
                    var imageFile = updateProductDto.ImageFiles[i];
                    var imageUrl = await _fileService.SaveImageAsync(imageFile);

                    var productImage = new ProductImage
                    {
                        ProductId = id,
                        ImageUrl = imageUrl,
                        DisplayOrder = startOrder + i,
                        IsMain = shouldSetAsMain && i == 0
                    };

                    await _productImageRepository.CreateAsync(productImage);

                    // Si es la primera imagen y no había otras, actualizar MainImageUrl
                    if (shouldSetAsMain && i == 0)
                    {
                        product.MainImageUrl = imageUrl;
                    }
                }
            }

            // Verificar estado final de las imágenes
            var finalImages = await _productImageRepository.GetByProductIdAsync(id);

            if (!finalImages.Any())
            {
                product.MainImageUrl = DEFAULT_PLACEHOLDER;
            }
            else
            {
                var mainImage = finalImages.FirstOrDefault(img => img.IsMain);

                if (mainImage != null)
                {
                    product.MainImageUrl = mainImage.ImageUrl;
                }
                else
                {
                    // Si no hay imagen principal establecida, usar la primera
                    var firstImage = finalImages.OrderBy(img => img.DisplayOrder).First();
                    firstImage.IsMain = true;
                    await _productImageRepository.UpdateAsync(firstImage.Id, firstImage);
                    product.MainImageUrl = firstImage.ImageUrl;
                }
            }

            var updatedProduct = await _productRepository.UpdateAsync(id, product);
            if (updatedProduct == null)
            {
                Log.Error("Failed to update product in repository: {ProductId}", id);
                return null;
            }

            var images = await _productImageRepository.GetByProductIdAsync(id);

            return new ProductDto
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                Stock = updatedProduct.Stock,
                Category = updatedProduct.Category,
                Brand = updatedProduct.Brand,
                Model = updatedProduct.Model,
                MainImageUrl = updatedProduct.MainImageUrl,
                Status = updatedProduct.Status,       
                DeletedAt = updatedProduct.DeletedAt,
                CreatedAt = updatedProduct.CreatedAt,
                UpdatedAt = updatedProduct.UpdatedAt,
                Images = images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ProductId = img.ProductId,
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    IsMain = img.IsMain,
                    CreatedAt = img.CreatedAt
                }).ToList()
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            var images = await _productImageRepository.GetByProductIdAsync(id);
            foreach (var image in images)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl) && !image.ImageUrl.StartsWith("http"))
                {
                    await _fileService.DeleteImageAsync(image.ImageUrl);
                }
                await _productImageRepository.DeleteAsync(image.Id);
            }

            return await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductListDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<IEnumerable<ProductListDto>> GetProductsByBrandAsync(string brand)
        {
            var products = await _productRepository.GetByBrandAsync(brand);
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<IEnumerable<ProductListDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchAsync(searchTerm);
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _productRepository.GetBrandsAsync();
        }

        public async Task<IEnumerable<ProductListDto>> FilterProductsAsync(
            string? category = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null)
        {
            var products = await _productRepository.FilterAsync(category, brand, minPrice, maxPrice, inStock);
            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Brand = p.Brand,
                Model = p.Model,
                MainImageUrl = p.MainImageUrl,
                Status = p.Status
            });
        }

        public async Task<object> GetProductMenuStructureAsync()
        {
            var structure = await _productRepository.GetMenuStructureAsync();

            return new
            {
                categories = structure.Select(category => new
                {
                    name = category.Key,
                    brands = category.Value.Select(brand => new
                    {
                        name = brand.Key,
                        models = brand.Value.OrderBy(m => m).ToList()
                    }).OrderBy(b => b.name).ToList()
                }).OrderBy(c => c.name).ToList()
            };
        }

        public async Task<object> GetProductStatsAsync()
        {
            return await _productRepository.GetProductStatsAsync();
        }
    }
}