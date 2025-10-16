using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllForAdminAsync()
        {
            return await _context.Products
                .AsNoTracking()  
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return null;

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Category = product.Category;
            existingProduct.Brand = product.Brand;
            existingProduct.Model = product.Model;
            existingProduct.Platform = product.Platform;
            existingProduct.MainImageUrl = product.MainImageUrl;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            // Soft delete - solo marca como inactivo
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.Category.ToLower() == category.ToLower())
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Obtiene productos por marca
        public async Task<IEnumerable<Product>> GetByBrandAsync(string brand)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.Brand.ToLower() == brand.ToLower())
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            var normalizedTerm = searchTerm.ToLower().Trim();

            return await _context.Products
                .Where(p => p.IsActive &&
                       (
                           p.Name.ToLower().Contains(normalizedTerm) ||
                           p.Brand.ToLower().Contains(normalizedTerm) ||
                           p.Model.ToLower().Contains(normalizedTerm) ||
                           p.Category.ToLower().Contains(normalizedTerm) ||
                           (p.Description != null && p.Description.ToLower().Contains(normalizedTerm)) ||

                           // Solo patrones específicos más importantes
                           p.Name.ToLower().StartsWith(normalizedTerm + "-") ||
                           p.Name.ToLower().Contains(" " + normalizedTerm + " ") ||
                           p.Name.ToLower().Contains(" " + normalizedTerm + "-")
                       ))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }

        // Filtrado avanzado
        public async Task<IEnumerable<Product>> FilterAsync(
            string? category = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null)
        {
            var query = _context.Products.Where(p => p.IsActive);

            // Filtra por categoría
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            // Filtra por marca
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand.ToLower() == brand.ToLower());
            }

            // Filtra por precio mínimo
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // Filtra por precio máximo
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Filtra por stock
            if (inStock.HasValue)
            {
                if (inStock.Value)
                {
                    query = query.Where(p => p.Stock > 0);
                }
                else
                {
                    query = query.Where(p => p.Stock == 0);
                }
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Estructura para menú jerárquico
        public async Task<Dictionary<string, Dictionary<string, List<string>>>> GetMenuStructureAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .GroupBy(p => new { p.Category, p.Brand, p.Model })
                .Select(g => new { g.Key.Category, g.Key.Brand, g.Key.Model, Count = g.Count() })
                .ToListAsync();

            var structure = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var product in products)
            {
                // Crea categoría si no existe
                if (!structure.ContainsKey(product.Category))
                {
                    structure[product.Category] = new Dictionary<string, List<string>>();
                }

                // Crea marca si no existe en la categoría
                if (!structure[product.Category].ContainsKey(product.Brand))
                {
                    structure[product.Category][product.Brand] = new List<string>();
                }

                // Agrega modelo si no está vacío
                if (!string.IsNullOrEmpty(product.Model) &&
                    !structure[product.Category][product.Brand].Contains(product.Model))
                {
                    structure[product.Category][product.Brand].Add(product.Model);
                }
            }

            return structure;
        }

        // Devuelve estadísticas de productos
        public async Task<object> GetProductStatsAsync()
        {
            var stats = await _context.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalProducts = g.Count(),
                    InStock = g.Count(p => p.Stock > 0),
                    OutOfStock = g.Count(p => p.Stock == 0),
                    AvgPrice = g.Average(p => p.Price),
                    MinPrice = g.Min(p => p.Price),
                    MaxPrice = g.Max(p => p.Price),
                    Brands = g.Select(p => p.Brand).Distinct().Count()
                })
                .ToListAsync();

            var totalStats = new
            {
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                TotalCategories = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .CountAsync(),
                TotalBrands = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => p.Brand)
                    .Distinct()
                    .CountAsync(),
                CategoryStats = stats
            };

            return totalStats;
        }
    }
}