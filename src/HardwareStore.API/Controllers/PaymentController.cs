using HardwareStore.Application.Common.Interfaces;
using HardwareStore.Application.Payments;
using HardwareStore.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HardwareStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IMercadoPagoService mercadoPagoService,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _mercadoPagoService = mercadoPagoService;
            _logger = logger;
            _configuration = configuration;
        }

        /// Crea una preferencia de pago de MercadoPago para una orden
        [HttpPost("mercadopago/create")]
        [Authorize]
        [EnableRateLimiting("general")]
        public async Task<ActionResult<MercadoPagoPaymentResponseDto>> CreateMercadoPagoPayment(
            [FromBody] CreateMercadoPagoPaymentDto dto)
        {
            try
            {
                var result = await _mercadoPagoService.CreatePaymentPreferenceAsync(
                    dto.OrderId,
                    dto.BackUrl
                );

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al crear preferencia de pago para orden {OrderId}", dto.OrderId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear preferencia de pago");
                return StatusCode(500, new { message = "Error al procesar el pago" });
            }
        }

        /// Webhook para recibir notificaciones de MercadoPago (IPN)
        [HttpPost("mercadopago/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoNotificationDto notification)
        {
            try
            {
                _logger.LogInformation(
                    "Webhook MercadoPago recibido: Action={Action}, Type={Type}, PaymentId={PaymentId}",
                    notification.Action,
                    notification.Type,
                    notification.Data?.Id
                );

                // Solo procesar notificaciones de pagos
                if (notification.Type != "payment" || string.IsNullOrEmpty(notification.Data?.Id))
                {
                    _logger.LogWarning("Webhook ignorado: tipo={Type} no soportado o sin ID", notification.Type);
                    return Ok(); // MercadoPago espera 200 OK siempre
                }

                // Validar firma del webhook
                var secretKey = _configuration["MercadoPago:WebhookSecret"];
                if (!string.IsNullOrEmpty(secretKey))
                {
                    var xSignature = Request.Headers["x-signature"].ToString();
                    var xRequestId = Request.Headers["x-request-id"].ToString();

                    if (string.IsNullOrEmpty(xSignature) || string.IsNullOrEmpty(xRequestId))
                    {
                        _logger.LogWarning("Webhook rechazado: headers de firma ausentes");
                        return Ok();
                    }

                    var validator = new MercadoPagoWebhookValidator(secretKey);
                    if (!validator.ValidateSignature(xSignature, xRequestId, notification.Data.Id))
                    {
                        _logger.LogWarning(
                            "Webhook rechazado: firma inválida. PaymentId={PaymentId}",
                            notification.Data.Id);
                        return Ok();
                    }

                    _logger.LogInformation("Firma de webhook validada correctamente");
                }
                else
                {
                    _logger.LogWarning("WebhookSecret no configurado, omitiendo validación de firma");
                }

                await _mercadoPagoService.ProcessWebhookNotificationAsync(notification.Data.Id);

                _logger.LogInformation("Webhook procesado exitosamente: PaymentId={PaymentId}", notification.Data.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar webhook de MercadoPago");
                // Siempre devolver 200 OK para que MercadoPago no reintente
                return Ok();
            }
        }

        /// Obtiene información de un pago de MercadoPago (para testing/debug)
        [HttpGet("mercadopago/{paymentId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPaymentInfo(string paymentId)
        {
            try
            {
                var paymentInfo = await _mercadoPagoService.GetPaymentInfoAsync(paymentId);
                return Ok(paymentInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del pago {PaymentId}", paymentId);
                return StatusCode(500, new { message = "Error al obtener información del pago" });
            }
        }
    }
}
