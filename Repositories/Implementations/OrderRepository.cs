using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.Id) ?? order;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> UpdateAsync(int id, Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
                return null;

            existingOrder.CustomerName = order.CustomerName;
            existingOrder.CustomerEmail = order.CustomerEmail;
            existingOrder.CustomerPhone = order.CustomerPhone;
            existingOrder.ShippingAddressId = order.ShippingAddressId;
            existingOrder.ShippingStreet = order.ShippingStreet;
            existingOrder.ShippingNumber = order.ShippingNumber;
            existingOrder.Total = order.Total;
            existingOrder.Status = order.Status;

            // Campos de pago
            existingOrder.PaymentMethod = order.PaymentMethod;
            existingOrder.PaymentReceiptUrl = order.PaymentReceiptUrl;
            existingOrder.PaymentReceiptUploadedAt = order.PaymentReceiptUploadedAt;
            existingOrder.PaymentApprovedAt = order.PaymentApprovedAt;
            existingOrder.ShippedAt = order.ShippedAt;
            existingOrder.DeliveredAt = order.DeliveredAt;
            existingOrder.AdminNotes = order.AdminNotes;
            existingOrder.TrackingNumber = order.TrackingNumber;
            existingOrder.ShippingProvider = order.ShippingProvider;

            existingOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> CancelOrderWithStockRestoreAsync(int orderId, Func<int, int, Task> restoreStockCallback)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await GetOrderWithItemsAsync(orderId);
                if (order == null || (order.Status != "pending_payment" && order.Status != "payment_rejected"))
                    return false;

                // Restaura stock usando callback
                foreach (var item in order.OrderItems)
                {
                    await restoreStockCallback(item.ProductId, item.Quantity);
                }

                // Elimina orden
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
