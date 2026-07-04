using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PriceFinderAI.Application.Interfaces;
using PriceFinderAI.Application.Options;

namespace PriceFinderAI.Infrastructure.Email;

// System.Net.Mail.SmtpClient yerine MailKit'e geçiş: yalnızca sağlayıcı OAuth2 gerektirirse
// veya teslim edilebilirlik sorun olursa gerekli (bkz. proje planı).
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var smtp = options.Value;

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            Credentials = new NetworkCredential(smtp.User, smtp.Password),
            EnableSsl = smtp.EnableSsl,
            Timeout = 15_000
        };

        using var message = new MailMessage(smtp.From, toEmail, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
