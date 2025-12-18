using HardwareStore.Application.Common.Interfaces;
using HardwareStore.Application.Payments;
using HardwareStore.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Common;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using MercadoPago.Resource.Payment;

namespace HardwareStore.Infrastructure.ExternalServices
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly string _accessToken;

        public MercadoPagoService(
            IOrderRepository orderRepository,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _accessToken = configuration["MercadoPago:AccessToken"]
                ?? throw new InvalidOperationException("MercadoPago AccessToken no configurado");

            // Configura el SDK de MercadoPago
            MercadoPagoConfig.AccessToken = _accessToken;
        }

        public async Task<MercadoPagoPaymentResponseDto> CreatePaymentPreferenceAsync(int orderId, string backUrl)
        {
            // Obtiene la orden
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Orden {orderId} no encontrada");

            if (order.Total <= 0)
                throw new InvalidOperationException("El total de la orden debe ser mayor a 0");

            // Crea items para MercadoPago
            var items = order.OrderItems.Select(item => new PreferenceItemRequest
            {
                Title = item.ProductName,
                Quantity = item.Quantity,
                CurrencyId = "ARS",  
                UnitPrice = item.UnitPrice
            }).ToList();

            // Determina el teléfono a usar (prioriza AuthorizedPersonPhone)
            var phoneNumber = !string.IsNullOrWhiteSpace(order.AuthorizedPersonPhone)
                ? order.AuthorizedPersonPhone
                : order.CustomerPhone ?? "";

            // Configura URLs de retorno
            var preferenceRequest = new PreferenceRequest
            {
                ExternalReference = orderId.ToString(),
                Items = items,
                Payer = new PreferencePayerRequest
                {
                    Name = order.CustomerName,
                    Email = order.CustomerEmail,
                    Phone = new PhoneRequest
                    {
                        AreaCode = "",
                        Number = phoneNumber
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{backUrl}/success?external_reference={orderId}",
                    Failure = $"{backUrl}/failure?external_reference={orderId}",
                    Pending = $"{backUrl}/pending?external_reference={orderId}"
                },
                StatementDescriptor = "HARDWARE_STORE",
                BinaryMode = true,
                NotificationUrl = _configuration["MercadoPago:WebhookUrl"]
            };

            // Crea la preferencia en MercadoPago
            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(preferenceRequest);

            // Actualiza la orden con el PreferenceId
            order.MercadoPagoPreferenceId = preference.Id;
            order.PaymentMethod = "mercadopago";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order.Id, order);

            return new MercadoPagoPaymentResponseDto
            {
                PreferenceId = preference.Id,
                InitPoint = preference.InitPoint,
                SandboxInitPoint = preference.SandboxInitPoint
            };
        }

        public async Task<MercadoPagoPaymentInfo> GetPaymentInfoAsync(string paymentId)
        {
            var client = new PaymentClient();
            Payment payment = await client.GetAsync(Convert.ToInt64(paymentId));

            return new MercadoPagoPaymentInfo
            {
                Id = payment.Id.ToString(),
                Status = payment.Status,
                StatusDetail = payment.StatusDetail,
                TransactionAmount = payment.TransactionAmount ?? 0,
                PaymentTypeId = payment.PaymentTypeId,
                ExternalReference = payment.ExternalReference
            };
        }

        public async Task ProcessWebhookNotificationAsync(string paymentId)
        {
            // Obtiene información del pago
            var paymentInfo = await GetPaymentInfoAsync(paymentId);

            // Busca la orden por ExternalReference
            if (string.IsNullOrEmpty(paymentInfo.ExternalReference))
                return;

            if (!int.TryParse(paymentInfo.ExternalReference, out int orderId))
                return;

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return;

            // Actualiza la orden según el estado del pago
            order.MercadoPagoPaymentId = paymentInfo.Id;
            order.MercadoPagoStatus = paymentInfo.Status;
            order.MercadoPagoPaymentType = paymentInfo.PaymentTypeId;
            order.UpdatedAt = DateTime.UtcNow;

            // Cambia el estado de la orden según el estado de MercadoPago
            switch (paymentInfo.Status?.ToLower())
            {
                case "approved":
                    order.Status = "paid";
                    order.PaymentApprovedAt = DateTime.UtcNow;
                    break;
                case "pending":
                case "in_process":
                    order.Status = "pending_payment";
                    order.PaymentSubmittedAt = DateTime.UtcNow;
                    break;
                case "rejected":
                case "cancelled":
                    order.Status = "payment_failed";
                    break;
                case "refunded":
                case "charged_back":
                    order.Status = "refunded";
                    break;
            }

            await _orderRepository.UpdateAsync(order.Id, order);
        }
    }
}
