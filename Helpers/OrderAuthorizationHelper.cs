using System.Security.Claims;

namespace EcommerceAPI.Helpers
{
    public static class OrderAuthorizationHelper
    {
        public static bool CanUserAccessOrder(ClaimsPrincipal user, int? orderUserId)
        {
            if (!user.Identity?.IsAuthenticated == true)
                return false;

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Los admin pueden acceder a cualquier orden
            if (userRole == "Admin")
                return true;

            // Los usuarios solo pueden acceder a sus propias órdenes
            if (int.TryParse(userIdClaim, out int userId))
            {
                return orderUserId == userId;
            }

            return false;
        }

        public static (bool IsValid, int UserId) GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return (true, userId);
            }

            return (false, 0);
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            return userRole == "Admin";
        }

        public static bool IsCustomer(ClaimsPrincipal user)
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            return userRole == "Customer";
        }

        public static (bool IsValid, int UserId, string Email, string Role) GetUserInfoFromClaims(ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated == true)
                return (false, 0, string.Empty, string.Empty);

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return (true, userId, email, role);
            }

            return (false, 0, string.Empty, string.Empty);
        }

        public static class AdminOperations
        {
            // Verifica si puede aprobar pagos
            public static bool CanApprovePayments(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede rechazar pagos
            public static bool CanRejectPayments(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede cambiar estado de órdenes
            public static bool CanUpdateOrderStatus(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede marcar como enviado
            public static bool CanMarkAsShipped(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede marcar como entregado
            public static bool CanMarkAsDelivered(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede ver todas las órdenes
            public static bool CanViewAllOrders(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
            // Verifica si puede ver órdenes por estado
            public static bool CanViewOrdersByStatus(ClaimsPrincipal user)
            {
                return IsAdmin(user);
            }
        }

        // Verifica permisos para operaciones de cliente
        public static class CustomerOperations
        {
            // Verifica si puede crear órdenes
            public static bool CanCreateOrders(ClaimsPrincipal user)
            {
                return true;
            }

            // Verifica si puede subir comprobantes
            public static bool CanUploadReceipts(ClaimsPrincipal user, int? orderUserId)
            {
                return CanUserAccessOrder(user, orderUserId);
            }

            // Verifica si puede ver sus propias órdenes
            public static bool CanViewOwnOrders(ClaimsPrincipal user)
            {
                return user.Identity?.IsAuthenticated == true;
            }

            // Verifica si puede ver comprobante de una orden
            public static bool CanViewReceipt(ClaimsPrincipal user, int? orderUserId)
            {
                return CanUserAccessOrder(user, orderUserId);
            }
        }

        // Genera contexto de autorización para logging
        public static string GetAuthorizationContext(ClaimsPrincipal user, string operation, int? orderId = null)
        {
            var userInfo = GetUserInfoFromClaims(user);
            var context = $"Operation={operation}, UserId={userInfo.UserId}, Role={userInfo.Role}";

            if (orderId.HasValue)
                context += $", OrderId={orderId}";

            return context;
        }

        // Valida que un usuario tenga una sesión válida
        public static bool HasValidSession(ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated == true)
                return false;

            // Verifica que tenga claims básicos
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = user.FindFirst(ClaimTypes.Email);
            var roleClaim = user.FindFirst(ClaimTypes.Role);

            return userIdClaim != null && emailClaim != null && roleClaim != null;
        }
    }
}
