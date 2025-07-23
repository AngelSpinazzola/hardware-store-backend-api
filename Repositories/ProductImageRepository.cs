using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ThenBy(pi => pi.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetByIdAsync(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public async Task<ProductImage> CreateAsync(ProductImage productImage)
        {
            productImage.CreatedAt = DateTime.UtcNow;
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
            return productImage;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            if (productImage == null)
                return false;

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetMainImageAsync(int productId, int imageId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Quita IsMain de todas las imágenes del producto
                var allImages = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .ToListAsync();

                foreach (var img in allImages)
                {
                    img.IsMain = false;
                }

                // Marca la nueva imagen como principal
                var mainImage = allImages.FirstOrDefault(pi => pi.Id == imageId);
                if (mainImage == null)
                    return false;

                mainImage.IsMain = true;

                // Actualiza MainImageUrl en Product
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    product.MainImageUrl = mainImage.ImageUrl;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateDisplayOrderAsync(int imageId, int displayOrder)
        {
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage == null)
                return false;

            productImage.DisplayOrder = displayOrder;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMultipleDisplayOrderAsync(Dictionary<int, int> imageOrders)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var kvp in imageOrders)
                {
                    var productImage = await _context.ProductImages.FindAsync(kvp.Key);
                    if (productImage != null)
                    {
                        productImage.DisplayOrder = kvp.Value;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<ProductImage?> GetMainImageAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsMain)
                .FirstOrDefaultAsync();
        }
    }
}
