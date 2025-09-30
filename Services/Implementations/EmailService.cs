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
                From = new MailAddress(_configuration["Email:From"], "NOVATECH"),
                Subject = "Recuperación de contraseña - NOVATECH",
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    </head>
                    <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
                            <tr>
                                <td align='center'>
                                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    
                                        <!-- Header -->
                                        <tr>
                                            <td style='background: #494b4d; padding: 40px 20px; text-align: center;'>
                                                <img src='https://res.cloudinary.com/dltfs92ie/image/upload/v1759254086/favicon-96x96_zbcayb.png' alt='NOVATECH' style='max-width: 96px; height: auto; margin-bottom: 15px;' />
                                                <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: bold;'>NOVATECH</h1>
                                            </td>
                                        </tr>
                    
                                        <!-- Content -->
                                        <tr>
                                            <td style='padding: 40px 30px;'>
                                                <h2 style='color: #333333; margin: 0 0 20px 0; font-size: 24px;'>Recuperación de contraseña</h2>
                                                <p style='color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                                    Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                                                </p>
                                                <p style='color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                                    Haz clic en el botón de abajo para crear una nueva contraseña:
                                                </p>
                            
                                                <!-- Button -->
                                                <table width='100%' cellpadding='0' cellspacing='0'>
                                                    <tr>
                                                        <td align='center' style='padding: 20px 0;'>
                                                            <a href='{resetLink}' style='background: linear-gradient(135deg, #f97316 0%, #ea580c 100%); color: #ffffff; padding: 16px 40px; text-decoration: none; border-radius: 50px; font-size: 16px; font-weight: bold; display: inline-block; box-shadow: 0 4px 15px rgba(249, 115, 22, 0.4);'>
                                                                Restablecer contraseña
                                                            </a>
                                                        </td>
                                                    </tr>
                                                </table>
                            
                                                <p style='color: #999999; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;'>
                                                    O copia y pega este enlace en tu navegador:
                                                </p>
                                                <p style='color: #f97316; font-size: 14px; word-break: break-all; margin: 10px 0 0 0;'>
                                                    {resetLink}
                                                </p>
                                            </td>
                                        </tr>
                    
                                        <!-- Footer -->
                                        <tr>
                                            <td style='background-color: #f8f9fa; padding: 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                                <p style='color: #999999; font-size: 14px; margin: 0 0 10px 0;'>
                                                    <strong>Este enlace expirará en 1 hora</strong>
                                                </p>
                                                <p style='color: #999999; font-size: 13px; margin: 0;'>
                                                    Si no solicitaste este cambio, puedes ignorar este correo.
                                                </p>
                                                <p style='color: #cccccc; font-size: 12px; margin: 20px 0 0 0;'>
                                                    © 2026 NOVATECH. Todos los derechos reservados.
                                                </p>
                                            </td>
                                        </tr>
                    
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var smtp = new SmtpClient(_configuration["Email:SmtpServer"])
            {
                Port = 587,
                Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            ),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 90000
            };

            await smtp.SendMailAsync(message);
        }
    }
}