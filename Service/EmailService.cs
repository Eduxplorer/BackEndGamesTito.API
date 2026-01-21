using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace BackEndGamesTito.API.Service
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // 1. Pega as configurações do appsettings.json
            var emailSettings = _configuration.GetSection("EmailSettings");

            // 2. Cria a mensagem (O envelope)
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailSettings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            // Aqui definimos que o corpo do email aceita HTML
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            // 3. Conecta no Gmail e envia (O envio)
            using var smtp = new SmtpClient();
            try
            {
                // Conecta no servidor do Gmail na porta 587 usando TLS (segurança)
                await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);

                // Faz o login
                await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);

                // Envia
                await smtp.SendAsync(email);
            }
            finally
            {
                // Garante que desconecta mesmo se der erro
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
