using HardwareStore.Application.Orders;
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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public OrderController(IOrderService orderService, IConfiguration configuration)
        {
            _orderService = orderService;
            _configuration = configuration;
        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid order creation attempt from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                return BadRequest(ModelState);
            }

            // Usa OrderAuthorizationHelper para obtener información del usuario
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
                if (userInfo.IsValid)
                {
                    userId = userInfo.UserId;
                    Log.Information("Order creation initiated by user: {UserId}", userId);
                }
            }
            else
            {
                Log.Information("Guest order creation from IP: {IP}",
                    HttpContext.Connection.RemoteIpAddress);
            }

            var order = await _orderService.CreateOrderAsync(createOrderDto, userId);

            Log.Information("Order created successfully: {OrderId} for user: {UserId}",
                order.Id, userId ?? 0);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // Devuelve orden por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            // Valida ID usando SecurityHelper
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID attempted: {OrderId} from IP: {IP}",
                    id, HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                Log.Warning("Order not found: {OrderId} requested by IP: {IP}",
                    id, HttpContext.Connection.RemoteIpAddress);
                return NotFound(new { message = "Orden no encontrada" });
            }

            // Usa OrderAuthorizationHelper para verificar permisos
            if (!OrderAuthorizationHelper.CanUserAccessOrder(User, order.UserId))
            {
                var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "GetOrder", id);
                Log.Warning("Unauthorized order access: {Context}", authContext);
                return Forbid("No tienes permisos para ver esta orden");
            }

            return Ok(order);
        }

        // Devuelve todas las órdenes (Solo Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            // Verifica permisos usando helper
            if (!OrderAuthorizationHelper.AdminOperations.CanViewAllOrders(User))
            {
                return Forbid("No tienes permisos para ver todas las órdenes");
            }

            // Lazy cleanup: cancelar órdenes expiradas
            await _orderService.CleanupExpiredOrdersAsync();

            var orders = await _orderService.GetAllOrdersAsync();

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "GetAllOrders");
            Log.Information("Admin accessed all orders: {Context}", authContext);

            return Ok(orders);
        }

        // Devuelve mis órdenes (Usuario autenticado)
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            // Verifica sesión válida
            if (!OrderAuthorizationHelper.HasValidSession(User))
            {
                return BadRequest(new { message = "Sesión inválida" });
            }

            var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
            if (!userInfo.IsValid)
            {
                Log.Warning("Invalid user ID in my-orders request: {Claim}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return BadRequest(new { message = "Usuario no válido" });
            }

            // Lazy cleanup: cancelar órdenes expiradas
            await _orderService.CleanupExpiredOrdersAsync();

            var orders = await _orderService.GetOrdersByUserIdAsync(userInfo.UserId);
            return Ok(orders);
        }

        // Devuelve órdenes por estado (Solo Admin)
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            // Verifica permisos
            if (!OrderAuthorizationHelper.AdminOperations.CanViewOrdersByStatus(User))
            {
                return Forbid("No tienes permisos para ver órdenes por estado");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new { message = "Estado requerido" });
            }

            // Sanitiza estado usando SecurityHelper
            var sanitizedStatus = SecurityHelper.SanitizeCategory(status);
            var limitedStatus = SecurityHelper.LimitStringLength(sanitizedStatus, 30);

            if (string.IsNullOrEmpty(limitedStatus))
            {
                return BadRequest(new { message = "Estado inválido" });
            }

            var orders = await _orderService.GetOrdersByStatusAsync(limitedStatus);

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "GetOrdersByStatus");
            Log.Information("Admin accessed orders by status: {Status}, {Context}", limitedStatus, authContext);

            return Ok(orders);
        }

        // Actualiza estado de orden (Solo Admin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateStatusDto)
        {
            // Validaciones usando helpers
            if (!SecurityHelper.IsValidId(id))
            {
                var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Warning("Invalid order ID for status update: {OrderId} by Admin: {AdminId}",
                    id, userInfo.UserId);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            if (!ModelState.IsValid)
            {
                var statusContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "UpdateOrderStatus", id);
                Log.Warning("Invalid status update attempt: {Context}", statusContext);
                return BadRequest(ModelState);
            }

            // Verifica permisos específicos
            if (!OrderAuthorizationHelper.AdminOperations.CanUpdateOrderStatus(User))
            {
                return Forbid("No tienes permisos para actualizar estados de órdenes");
            }

            var result = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto.Status, updateStatusDto.AdminNotes);
            if (!result)
            {
                Log.Warning("Order not found for status update: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada" });
            }

            var statusUpdateContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "UpdateOrderStatus", id);
            Log.Information("Order status updated: Status={Status}, {Context}", updateStatusDto.Status, statusUpdateContext);

            return Ok(new { message = "Estado de la orden actualizado correctamente" });
        }

        // Sube comprobante de pago
        [HttpPost("{id}/payment-receipt")]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("upload")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadPaymentReceipt(int id, [FromForm] IFormFile receiptFile)
        {
            // Validaciones básicas
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for receipt upload: {OrderId}", id);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            if (receiptFile == null || receiptFile.Length == 0)
            {
                Log.Warning("Empty file upload attempt for order: {OrderId}", id);
                return BadRequest(new { message = "No se proporcionó archivo de comprobante" });
            }

            // Valida archivo usando FileValidationHelper
            var fileValidation = FileValidationHelper.ValidatePaymentReceipt(receiptFile);
            if (!fileValidation.IsValid)
            {
                Log.Warning("Receipt file validation failed for order {OrderId}: {Reason}",
                    id, fileValidation.ErrorMessage);
                return BadRequest(new { message = fileValidation.ErrorMessage });
            }

            var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);

            // Verifica permisos usando helper
            if (!OrderAuthorizationHelper.CustomerOperations.CanUploadReceipts(User, userInfo.UserId) &&
                !await _orderService.CanUserAccessOrderAsync(id, userInfo.UserId))
            {
                var uploadContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "UploadReceipt", id);
                Log.Warning("Unauthorized receipt upload: {Context}", uploadContext);
                return Forbid("No tienes permisos para subir comprobante a esta orden");
            }

            var result = await _orderService.UploadPaymentReceiptAsync(id, receiptFile);
            if (!result)
            {
                Log.Warning("Receipt upload failed - invalid order state: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada o no está en estado válido para subir comprobante" });
            }

            var uploadSuccessContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "UploadReceipt", id);
            Log.Information("Payment receipt uploaded: {Context}", uploadSuccessContext);

            return Ok(new { message = "Comprobante de pago subido correctamente. Tu orden está ahora en revisión." });
        }

        // Devuelve órdenes pendientes de revisión (Solo Admin)
        [HttpGet("pending-review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrdersPendingReview()
        {
            var orders = await _orderService.GetOrdersByStatusAsync("payment_submitted");

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "GetPendingReview");
            Log.Information("Admin accessed pending review orders: {Context}", authContext);

            return Ok(orders);
        }

        // Aprobar pago (Solo Admin)
        [HttpPut("{id}/approve-payment")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> ApprovePayment(int id, [FromBody] AdminActionDto adminActionDto)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Warning("Invalid order ID for payment approval: {OrderId} by Admin: {AdminId}",
                    id, userInfo.UserId);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            // Verifica permisos específicos
            if (!OrderAuthorizationHelper.AdminOperations.CanApprovePayments(User))
            {
                return Forbid("No tienes permisos para aprobar pagos");
            }

            var result = await _orderService.ApprovePaymentAsync(id, adminActionDto.AdminNotes);
            if (!result)
            {
                Log.Warning("Payment approval failed - order not found or invalid state: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada o no está en estado válido para aprobar" });
            }

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "ApprovePayment", id);
            Log.Information("Payment approved: {Context}", authContext);

            return Ok(new { message = "Pago aprobado correctamente" });
        }

        // Rechazar pago (Solo Admin)
        [HttpPut("{id}/reject-payment")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> RejectPayment(int id, [FromBody] AdminActionDto adminActionDto)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Warning("Invalid order ID for payment rejection: {OrderId} by Admin: {AdminId}",
                    id, userInfo.UserId);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            if (string.IsNullOrWhiteSpace(adminActionDto.AdminNotes))
            {
                return BadRequest(new { message = "Se requiere especificar el motivo del rechazo" });
            }

            // Verifica permisos específicos
            if (!OrderAuthorizationHelper.AdminOperations.CanRejectPayments(User))
            {
                return Forbid("No tienes permisos para rechazar pagos");
            }

            var result = await _orderService.RejectPaymentAsync(id, adminActionDto.AdminNotes);
            if (!result)
            {
                Log.Warning("Payment rejection failed - order not found or invalid state: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada o no está en estado válido para rechazar" });
            }

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "RejectPayment", id);
            Log.Warning("Payment rejected: {Context}, Reason={Reason}", authContext, adminActionDto.AdminNotes);

            return Ok(new { message = "Pago rechazado" });
        }

        // Marca como enviado (Solo Admin)
        [HttpPut("{id}/mark-shipped")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> MarkAsShipped(int id, [FromBody] ShippingInfoDto shippingInfoDto)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Warning("Invalid order ID for shipping: {OrderId} by Admin: {AdminId}",
                    id, userInfo.UserId);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            // Verifica permisos específicos
            if (!OrderAuthorizationHelper.AdminOperations.CanMarkAsShipped(User))
            {
                return Forbid("No tienes permisos para marcar órdenes como enviadas");
            }

            var result = await _orderService.MarkAsShippedAsync(id, shippingInfoDto.TrackingNumber,
                shippingInfoDto.ShippingProvider, shippingInfoDto.AdminNotes);
            if (!result)
            {
                Log.Warning("Mark as shipped failed - order not found or invalid state: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada o no está en estado válido para marcar como enviado" });
            }

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "MarkShipped", id);
            Log.Information("Order marked as shipped: Tracking={Tracking}, {Context}",
                shippingInfoDto.TrackingNumber, authContext);

            return Ok(new { message = "Orden marcada como enviada correctamente" });
        }

        // Marca como entregado (Solo Admin)
        [HttpPut("{id}/mark-delivered")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> MarkAsDelivered(int id, [FromBody] AdminActionDto adminActionDto)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);
                Log.Warning("Invalid order ID for delivery: {OrderId} by Admin: {AdminId}",
                    id, userInfo.UserId);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            // Verifica permisos específicos
            if (!OrderAuthorizationHelper.AdminOperations.CanMarkAsDelivered(User))
            {
                return Forbid("No tienes permisos para marcar órdenes como entregadas");
            }

            var result = await _orderService.MarkAsDeliveredAsync(id, adminActionDto.AdminNotes);
            if (!result)
            {
                Log.Warning("Mark as delivered failed - order not found or invalid state: OrderId={OrderId}", id);
                return NotFound(new { message = "Orden no encontrada o no está en estado válido para marcar como entregado" });
            }

            var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "MarkDelivered", id);
            Log.Information("Order marked as delivered: {Context}", authContext);

            return Ok(new { message = "Orden marcada como entregada correctamente" });
        }

        // Devuelve URL del comprobante de pago
        [HttpGet("{id}/payment-receipt")]
        [Authorize]
        public async Task<IActionResult> GetPaymentReceipt(int id)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for receipt access: {OrderId}", id);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);

            // Verifica permisos usando helper
            if (!OrderAuthorizationHelper.CustomerOperations.CanViewReceipt(User, userInfo.UserId) ||
                !await _orderService.CanUserAccessOrderAsync(id, userInfo.UserId))
            {
                var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "GetReceipt", id);
                Log.Warning("Unauthorized receipt access: {Context}", authContext);
                return Forbid("No tienes permisos para ver el comprobante de esta orden");
            }

            var receiptUrl = await _orderService.GetPaymentReceiptUrlAsync(id);
            if (string.IsNullOrEmpty(receiptUrl))
            {
                return NotFound(new { message = "No se encontró comprobante para esta orden" });
            }

            return Ok(new { receiptUrl });
        }

        // Descarga comprobante de pago
        [HttpGet("{id}/download-receipt")]
        [Authorize]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for receipt download: {OrderId}", id);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);

            if (!OrderAuthorizationHelper.CanUserAccessOrder(User, userInfo.UserId) ||
                !await _orderService.CanUserAccessOrderAsync(id, userInfo.UserId))
            {
                var downloadContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "DownloadReceipt", id);
                Log.Warning("Unauthorized receipt download: {Context}", downloadContext);
                return Forbid();
            }

            var receiptUrl = await _orderService.GetPaymentReceiptUrlAsync(id);
            if (string.IsNullOrEmpty(receiptUrl))
            {
                return NotFound();
            }

            var downloadSuccessContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "DownloadReceipt", id);
            Log.Information("Receipt download initiated: {Context}", downloadSuccessContext);

            return Redirect(receiptUrl);
        }

        // Ver comprobante de pago
        [HttpGet("{id}/view-receipt")]
        [Authorize]
        public async Task<IActionResult> ViewReceipt(int id)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for receipt view: {OrderId}", id);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            var userInfo = OrderAuthorizationHelper.GetUserInfoFromClaims(User);

            if (!OrderAuthorizationHelper.CanUserAccessOrder(User, userInfo.UserId) ||
                !await _orderService.CanUserAccessOrderAsync(id, userInfo.UserId))
            {
                var viewContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "ViewReceipt", id);
                Log.Warning("Unauthorized receipt view: {Context}", viewContext);
                return Forbid();
            }

            var receiptUrl = await _orderService.GetPaymentReceiptUrlAsync(id);
            if (string.IsNullOrEmpty(receiptUrl))
            {
                return NotFound();
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(receiptUrl);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to access receipt file: OrderId={OrderId}, Status={Status}",
                    id, response.StatusCode);
                return StatusCode(500, new { message = "Error accessing file" });
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            var contentType = "application/octet-stream";
            if (receiptUrl.Contains(".pdf"))
                contentType = "application/pdf";
            else if (receiptUrl.Contains(".jpg") || receiptUrl.Contains(".jpeg"))
                contentType = "image/jpeg";
            else if (receiptUrl.Contains(".png"))
                contentType = "image/png";

            Response.Headers["Content-Disposition"] = "inline";
            Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

            var viewSuccessContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "ViewReceipt", id);
            Log.Information("Receipt viewed: {Context}", viewSuccessContext);

            return File(fileBytes, contentType);
        }

        // Cancela orden (Usuario autenticado)
        [HttpDelete("{id}/cancel")]
        [Authorize]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            if (!SecurityHelper.IsValidId(id))
            {
                Log.Warning("Invalid order ID for cancellation: {OrderId}", id);
                return BadRequest(new { message = "ID de orden inválido" });
            }

            var userInfo = OrderAuthorizationHelper.GetUserIdFromClaims(User);
            if (!userInfo.IsValid)
            {
                return BadRequest(new { message = "Usuario no válido" });
            }

            // Verifica que el usuario pueda cancelar la orden
            if (!await _orderService.CanUserCancelOrderAsync(id, userInfo.UserId))
            {
                var authContext = OrderAuthorizationHelper.GetAuthorizationContext(User, "CancelOrder", id);
                Log.Warning("Unauthorized order cancellation: {Context}", authContext);
                return Forbid("No tienes permisos para cancelar esta orden");
            }

            var result = await _orderService.CancelOrderAsync(id);
            if (!result)
            {
                Log.Warning("Order cancellation failed: OrderId={OrderId}", id);
                return BadRequest(new { message = "No se pudo cancelar la orden" });
            }

            Log.Information("Order cancelled: OrderId={OrderId}, UserId={UserId}", id, userInfo.UserId);
            return Ok(new { message = "Orden cancelada correctamente" });
        }

        // Endpoint para limpieza automática de órdenes expiradas (GitHub Actions / Cron)
        [HttpPost("cleanup-expired")]
        [AllowAnonymous]
        public async Task<IActionResult> CleanupExpiredOrders(
            [FromHeader(Name = "X-Cleanup-Key")] string cleanupKey)
        {
            var expectedKey = _configuration["CleanupApiKey"];

            if (string.IsNullOrEmpty(cleanupKey) || cleanupKey != expectedKey)
            {
                Log.Warning("Unauthorized cleanup attempt from IP {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Invalid API key" });
            }

            var count = await _orderService.CleanupExpiredOrdersAsync();

            Log.Information("Cleanup completed: {Count} orders cancelled", count);

            return Ok(new
            {
                success = true,
                cancelledCount = count,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
