using PriceFinderAI.Application.Interfaces;

namespace PriceFinderAI.Tests;

public sealed class FakeEmailSender : IEmailSender
{
    public List<(string ToEmail, string Subject, string HtmlBody)> SentEmails { get; } = [];

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}

public sealed class ThrowingEmailSender : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("SMTP gönderimi başarısız (test).");
}
