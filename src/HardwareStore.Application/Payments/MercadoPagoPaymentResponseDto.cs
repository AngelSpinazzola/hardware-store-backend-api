namespace HardwareStore.Application.Payments
{
    public class MercadoPagoPaymentResponseDto
    {
        public string PreferenceId { get; set; }
        public string InitPoint { get; set; }  // URL para redirigir al usuario al checkout de MP
        public string SandboxInitPoint { get; set; }  // URL para Sandbox
    }
}
