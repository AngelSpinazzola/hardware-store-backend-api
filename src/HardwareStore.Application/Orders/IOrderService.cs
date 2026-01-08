using Microsoft.AspNetCore.Http;

namespace HardwareStore.Application.Orders
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, int? userId = null);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(string status);
        Task<bool> UpdateOrderStatusAsync(int id, string status, string? adminNotes = null);
        Task<bool> UploadPaymentReceiptAsync(int orderId, IFormFile receiptFile);
        Task<bool> ApprovePaymentAsync(int orderId, string? adminNotes = null);
        Task<bool> RejectPaymentAsync(int orderId, string adminNotes);
        Task<bool> MarkAsShippedAsync(int orderId, string trackingNumber, string shippingProvider, string? adminNotes = null);
        Task<bool> MarkAsDeliveredAsync(int orderId, string? adminNotes = null);
        Task<bool> CanUserAccessOrderAsync(int orderId, int userId);
        Task<string?> GetPaymentReceiptUrlAsync(int orderId);
        Task<bool> CanUserCancelOrderAsync(int orderId, int userId);
        Task<bool> CancelOrderAsync(int orderId);
        Task<int> CleanupExpiredOrdersAsync();
    }
}
