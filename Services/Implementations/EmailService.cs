using EcommerceAPI.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace EcommerceAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = "Recuperación de contraseña - NOVATECH",
                Body = $@"
                    <h2>Recuperación de contraseña</h2>
                    <p>Has solicitado restablecer tu contraseña.</p>
                    <p>Haz clic en el siguiente enlace:</p>
                    <a href='{resetLink}'>Restablecer contraseña</a>
                    <p>Este enlace expirará en 1 hora.</p>
                ",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var smtp = new SmtpClient(_configuration["Email:SmtpServer"])
            {
                Port = int.Parse(_configuration["Email:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                ),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}