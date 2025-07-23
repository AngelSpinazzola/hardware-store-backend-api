using EcommerceAPI.DTOs;
using EcommerceAPI.DTOs.Orders;
using EcommerceAPI.Helpers;
using EcommerceAPI.Models;
using EcommerceAPI.Repositories;
using Serilog;

namespace EcommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IFileService fileService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _fileService = fileService;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, int? userId = null)
        {
            // Valida datos de entrada usando SecurityHelper
            var sanitizedCustomerName = SecurityHelper.SanitizeInput(createOrderDto.CustomerName);
            var emailValidation = SecurityHelper.ValidateAndSanitizeEmail(createOrderDto.CustomerEmail);

            if (!emailValidation.IsValid)
            {
                throw new ArgumentException("Email inválido");
            }

            if (string.IsNullOrEmpty(sanitizedCustomerName))
            {
                throw new ArgumentException("Nombre de cliente requerido");
            }

            if (!SecurityHelper.IsValidId(createOrderDto.ShippingAddressId))
            {
                throw new ArgumentException("ID de dirección de envío inválido");
            }

            // Valida que todos los productos existan y tengan stock suficiente
            var orderItems = new List<OrderItem>();
            decimal total = 0;

            foreach (var item in createOrderDto.Items)
            {
                // Valida ProductId
                if (!SecurityHelper.IsValidId(item.ProductId))
                {
                    throw new ArgumentException($"ID de producto inválido: {item.ProductId}");
                }

                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Cantidad inválida para producto {item.ProductId}");
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    Log.Warning("Product not found during order creation: ProductId={ProductId}", item.ProductId);
                    throw new ArgumentException($"Producto con ID {item.ProductId} no encontrado");
                }

                if (product.Stock < item.Quantity)
                {
                    Log.Warning("Insufficient stock: ProductId={ProductId}, Available={Available}, Requested={Requested}",
                        item.ProductId, product.Stock, item.Quantity);
                    throw new ArgumentException($"Stock insuficiente para {product.Name}. Disponible: {product.Stock}, Solicitado: {item.Quantity}");
                }

                if (!product.IsActive)
                {
                    Log.Warning("Inactive product in order: ProductId={ProductId}", item.ProductId);
                    throw new ArgumentException($"El producto {product.Name} no está disponible");
                }

                var subtotal = product.Price * item.Quantity;
                total += subtotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = subtotal,
                    ProductName = product.Name,
                    ProductImageUrl = product.MainImageUrl,
                    ProductBrand = product.Brand,      // Brand para historial
                    ProductModel = product.Model       // Model para historial
                });
            }

            // Crea la orden
            var order = new Order
            {
                CustomerName = sanitizedCustomerName,
                CustomerEmail = emailValidation.SanitizedEmail,
                CustomerPhone = SecurityHelper.SanitizeInput(createOrderDto.CustomerPhone),
                ShippingAddressId = createOrderDto.ShippingAddressId,
                Total = total,
                Status = OrderStatus.PendingPayment,
                PaymentMethod = "bank_transfer",
                UserId = userId,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Descuenta stock de los productos
            foreach (var item in createOrderDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product.Id, product);

                    Log.Information("Stock updated: ProductId={ProductId}, NewStock={NewStock}",
                        product.Id, product.Stock);
                }
            }

            Log.Information("Order created successfully: OrderId={OrderId}, Total={Total}, ItemsCount={ItemsCount}",
                createdOrder.Id, total, orderItems.Count);

            var finalOrder = await _orderRepository.GetByIdAsync(createdOrder.Id);
            return MapToOrderDto(finalOrder!);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            // Valida ID
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID requested: {OrderId}", id);
                return null;
            }

            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToOrderDto(order);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            Log.Information("Retrieved all orders: Count={Count}", orders.Count());
            return orders.Select(MapToOrderSummaryDto);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByUserIdAsync(int userId)
        {
            // Valida UserId
            if (!SecurityHelper.IsValidId(userId))
            {
                Log.Warning("Invalid user ID for orders: {UserId}", userId);
                return Enumerable.Empty<OrderSummaryDto>();
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            Log.Information("Retrieved orders for user: UserId={UserId}, Count={Count}", userId, orders.Count());
            return orders.Select(MapToOrderSummaryDto);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(string status)
        {
            var sanitizedStatus = SecurityHelper.SanitizeCategory(status);
            if (string.IsNullOrEmpty(sanitizedStatus))
            {
                Log.Warning("Invalid order status: {Status}", status);
                return Enumerable.Empty<OrderSummaryDto>();
            }

            if (!OrderStatus.IsValidStatus(sanitizedStatus))
            {
                Log.Warning("Unknown order status: {Status}", sanitizedStatus);
                return Enumerable.Empty<OrderSummaryDto>();
            }

            var orders = await _orderRepository.GetByStatusAsync(sanitizedStatus);
            Log.Information("Retrieved orders by status: Status={Status}, Count={Count}", sanitizedStatus, orders.Count());
            return orders.Select(MapToOrderSummaryDto);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status, string? adminNotes = null)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for status update: {OrderId}", id);
                return false;
            }

            var sanitizedStatus = SecurityHelper.SanitizeCategory(status);
            if (!OrderStatus.IsValidStatus(sanitizedStatus))
            {
                Log.Warning("Invalid order status for update: {Status}", status);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                Log.Warning("Order not found for status update: {OrderId}", id);
                return false;
            }

            var oldStatus = order.Status;
            order.Status = sanitizedStatus;
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                order.AdminNotes = SecurityHelper.SanitizeInput(adminNotes);
            }

            var result = await _orderRepository.UpdateAsync(order.Id, order) != null;

            if (result)
            {
                Log.Information("Order status updated: OrderId={OrderId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
                    id, oldStatus, sanitizedStatus);
            }

            return result;
        }

        public async Task<bool> UploadPaymentReceiptAsync(int orderId, IFormFile receiptFile)
        {
            if (!SecurityHelper.IsValidId(orderId))
            {
                Log.Warning("Invalid order ID for receipt upload: {OrderId}", orderId);
                return false;
            }

            // Valida archivo usando FileValidationHelper
            var fileValidation = FileValidationHelper.ValidatePaymentReceipt(receiptFile);
            if (!fileValidation.IsValid)
            {
                Log.Warning("Receipt file validation failed for order {OrderId}: {Error}",
                    orderId, fileValidation.ErrorMessage);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null || (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.PaymentRejected))
            {
                Log.Warning("Invalid order state for receipt upload: OrderId={OrderId}, Status={Status}",
                    orderId, order?.Status ?? "null");
                return false;
            }

            try
            {
                // Guarda el archivo del comprobante
                var receiptUrl = await _fileService.SaveImageAsync(receiptFile, "receipts");

                // Actualiza la orden
                order.PaymentReceiptUrl = receiptUrl;
                order.PaymentReceiptUploadedAt = DateTime.UtcNow;
                order.Status = OrderStatus.PaymentSubmitted;
                order.UpdatedAt = DateTime.UtcNow;

                var updatedOrder = await _orderRepository.UpdateAsync(order.Id, order);

                if (updatedOrder != null)
                {
                    Log.Information("Payment receipt uploaded successfully: OrderId={OrderId}, FileSize={FileSize}",
                        orderId, receiptFile.Length);
                }

                return updatedOrder != null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error uploading payment receipt: OrderId={OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> ApprovePaymentAsync(int orderId, string? adminNotes = null)
        {
            if (!SecurityHelper.IsValidId(orderId))
            {
                Log.Warning("Invalid order ID for payment approval: {OrderId}", orderId);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.PaymentSubmitted)
            {
                Log.Warning("Invalid order state for payment approval: OrderId={OrderId}, Status={Status}",
                    orderId, order?.Status ?? "null");
                return false;
            }

            order.Status = OrderStatus.PaymentApproved;
            order.PaymentApprovedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                order.AdminNotes = SecurityHelper.SanitizeInput(adminNotes);
            }

            var updatedOrder = await _orderRepository.UpdateAsync(order.Id, order);

            if (updatedOrder != null)
            {
                Log.Information("Payment approved: OrderId={OrderId}", orderId);
            }

            return updatedOrder != null;
        }

        public async Task<bool> RejectPaymentAsync(int orderId, string adminNotes)
        {
            if (!SecurityHelper.IsValidId(orderId))
            {
                Log.Warning("Invalid order ID for payment rejection: {OrderId}", orderId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(adminNotes))
            {
                Log.Warning("Admin notes required for payment rejection: OrderId={OrderId}", orderId);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.PaymentSubmitted)
            {
                Log.Warning("Invalid order state for payment rejection: OrderId={OrderId}, Status={Status}",
                    orderId, order?.Status ?? "null");
                return false;
            }

            order.Status = OrderStatus.PaymentRejected;
            order.AdminNotes = SecurityHelper.SanitizeInput(adminNotes);
            order.UpdatedAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order.Id, order);

            if (updatedOrder != null)
            {
                Log.Warning("Payment rejected: OrderId={OrderId}, Reason={Reason}", orderId, adminNotes);
            }

            return updatedOrder != null;
        }

        public async Task<bool> MarkAsShippedAsync(int orderId, string trackingNumber, string shippingProvider, string? adminNotes = null)
        {
            // Valida parámetros
            if (!SecurityHelper.IsValidId(orderId))
            {
                Log.Warning("Invalid order ID for shipping: {OrderId}", orderId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(trackingNumber) || string.IsNullOrWhiteSpace(shippingProvider))
            {
                Log.Warning("Tracking number and shipping provider required: OrderId={OrderId}", orderId);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.PaymentApproved)
            {
                Log.Warning("Invalid order state for shipping: OrderId={OrderId}, Status={Status}",
                    orderId, order?.Status ?? "null");
                return false;
            }

            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = SecurityHelper.SanitizeInput(trackingNumber);
            order.ShippingProvider = SecurityHelper.SanitizeInput(shippingProvider);
            order.ShippedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                order.AdminNotes = SecurityHelper.SanitizeInput(adminNotes);
            }

            var updatedOrder = await _orderRepository.UpdateAsync(order.Id, order);

            if (updatedOrder != null)
            {
                Log.Information("Order marked as shipped: OrderId={OrderId}, Tracking={Tracking}, Provider={Provider}",
                    orderId, trackingNumber, shippingProvider);
            }

            return updatedOrder != null;
        }

        public async Task<bool> MarkAsDeliveredAsync(int orderId, string? adminNotes = null)
        {
            if (!SecurityHelper.IsValidId(orderId))
            {
                Log.Warning("Invalid order ID for delivery: {OrderId}", orderId);
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Shipped)
            {
                Log.Warning("Invalid order state for delivery: OrderId={OrderId}, Status={Status}",
                    orderId, order?.Status ?? "null");
                return false;
            }

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(adminNotes))
            {
                order.AdminNotes = SecurityHelper.SanitizeInput(adminNotes);
            }

            var updatedOrder = await _orderRepository.UpdateAsync(order.Id, order);

            if (updatedOrder != null)
            {
                Log.Information("Order marked as delivered: OrderId={OrderId}", orderId);
            }

            return updatedOrder != null;
        }

        public async Task<bool> CanUserAccessOrderAsync(int orderId, int userId)
        {
            if (!SecurityHelper.AreValidIds(orderId, userId))
            {
                return false;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            return order?.UserId == userId;
        }

        public async Task<string?> GetPaymentReceiptUrlAsync(int orderId)
        {
            if (!SecurityHelper.IsValidId(orderId))
            {
                return null;
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            return order?.PaymentReceiptUrl;
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                ShippingAddressId = order.ShippingAddressId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,

                // Información de dirección copiada para historial
                ShippingAddressType = order.ShippingAddressType,
                ShippingStreet = order.ShippingStreet,
                ShippingNumber = order.ShippingNumber,
                ShippingFloor = order.ShippingFloor,
                ShippingApartment = order.ShippingApartment,
                ShippingTower = order.ShippingTower,
                ShippingBetweenStreets = order.ShippingBetweenStreets,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingProvince = order.ShippingProvince,
                ShippingCity = order.ShippingCity,
                ShippingObservations = order.ShippingObservations,

                // Información de persona autorizada
                AuthorizedPersonFirstName = order.AuthorizedPersonFirstName,
                AuthorizedPersonLastName = order.AuthorizedPersonLastName,
                AuthorizedPersonPhone = order.AuthorizedPersonPhone,
                AuthorizedPersonDni = order.AuthorizedPersonDni,

                Total = order.Total,
                Status = order.Status,
                StatusDescription = OrderStatus.GetStatusDescription(order.Status),
                PaymentMethod = order.PaymentMethod,
                PaymentReceiptUrl = order.PaymentReceiptUrl,
                PaymentReceiptUploadedAt = order.PaymentReceiptUploadedAt,
                PaymentApprovedAt = order.PaymentApprovedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                AdminNotes = order.AdminNotes,
                TrackingNumber = order.TrackingNumber,
                ShippingProvider = order.ShippingProvider,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal,
                    ProductName = oi.ProductName,
                    ProductImageUrl = oi.ProductImageUrl,
                    ProductBrand = oi.ProductBrand,    
                    ProductModel = oi.ProductModel    
                }).ToList()
            };
        }

        private OrderSummaryDto MapToOrderSummaryDto(Order order)
        {
            return new OrderSummaryDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Total = order.Total,
                Status = order.Status,
                StatusDescription = OrderStatus.GetStatusDescription(order.Status),
                HasPaymentReceipt = !string.IsNullOrEmpty(order.PaymentReceiptUrl),
                PaymentReceiptUrl = order.PaymentReceiptUrl,
                PaymentReceiptUploadedAt = order.PaymentReceiptUploadedAt,
                TrackingNumber = order.TrackingNumber,
                CreatedAt = order.CreatedAt,
                ItemsCount = order.OrderItems?.Count ?? 0,
                ShippingCity = order.ShippingCity,
                ShippingProvince = order.ShippingProvince
            };
        }
    }
}
