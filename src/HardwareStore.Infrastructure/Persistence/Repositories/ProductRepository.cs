using HardwareStore.Domain.Enums;
﻿using HardwareStore.Infrastructure.Persistence;
using HardwareStore.Domain.Entities;
using HardwareStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HardwareStore.Infrastructure.Persistence.Repositories
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
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllForAdminAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.Status != ProductStatus.Deleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status != ProductStatus.Deleted);
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
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Brand = product.Brand;
            existingProduct.Model = product.Model;
            existingProduct.Platform = product.Platform;
            existingProduct.MainImageUrl = product.MainImageUrl;
            existingProduct.Status = product.Status;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recargar con la navegación Category
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            // Soft delete - solo marca como inactivo
            product.Status = ProductStatus.Deleted; 
            product.DeletedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active && EF.Functions.Like(p.Category.Name, category))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Obtiene productos por marca
        public async Task<IEnumerable<Product>> GetByBrandAsync(string brand)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active && EF.Functions.Like(p.Brand, brand))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            var normalizedTerm = searchTerm.Trim().ToLower();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active &&
                       (
                           EF.Functions.Like(p.Name.ToLower(), $"%{normalizedTerm}%") ||
                           EF.Functions.Like(p.Brand.ToLower(), $"%{normalizedTerm}%") ||
                           EF.Functions.Like(p.Model.ToLower(), $"%{normalizedTerm}%") ||
                           EF.Functions.Like(p.Category.Name.ToLower(), $"%{normalizedTerm}%") ||
                           (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%{normalizedTerm}%")) ||

                           // Solo patrones específicos más importantes
                           EF.Functions.Like(p.Name.ToLower(), $"{normalizedTerm}-%") ||
                           EF.Functions.Like(p.Name.ToLower(), $"% {normalizedTerm} %") ||
                           EF.Functions.Like(p.Name.ToLower(), $"% {normalizedTerm}-%")
                       ))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active && p.Category != null)
                .Select(p => p.Category.Name)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _context.Products
                .Where(p => p.Status == ProductStatus.Active && !string.IsNullOrEmpty(p.Brand))
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
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Active);

            // Filtra por categoría
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => EF.Functions.Like(p.Category.Name, category));
            }

            // Filtra por marca
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => EF.Functions.Like(p.Brand, brand));
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
                .Where(p => p.Status == ProductStatus.Active)
                .GroupBy(p => new { p.Category, p.Brand, p.Model })
                .Select(g => new { g.Key.Category, g.Key.Brand, g.Key.Model, Count = g.Count() })
                .ToListAsync();

            var structure = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var product in products)
            {
                var categoryName = product.Category.Name;

                // Crea categoría si no existe
                if (!structure.ContainsKey(categoryName))
                {
                    structure[categoryName] = new Dictionary<string, List<string>>();
                }

                // Crea marca si no existe en la categoría
                if (!structure[categoryName].ContainsKey(product.Brand))
                {
                    structure[categoryName][product.Brand] = new List<string>();
                }

                // Agrega modelo si no está vacío
                if (!string.IsNullOrEmpty(product.Model) &&
                    !structure[categoryName][product.Brand].Contains(product.Model))
                {
                    structure[categoryName][product.Brand].Add(product.Model);
                }
            }

            return structure;
        }

        // Devuelve estadísticas de productos
        public async Task<object> GetProductStatsAsync()
        {
            var stats = await _context.Products
                .Where(p => p.Status == ProductStatus.Active)
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
                TotalProducts = await _context.Products.CountAsync(p => p.Status == ProductStatus.Active),
                TotalCategories = await _context.Products
                    .Where(p => p.Status == ProductStatus.Active)
                    .Select(p => p.Category)
                    .Distinct()
                    .CountAsync(),
                TotalBrands = await _context.Products
                    .Where(p => p.Status == ProductStatus.Active)
                    .Select(p => p.Brand)
                    .Distinct()
                    .CountAsync(),
                CategoryStats = stats
            };

            return totalStats;
        }
    }
}