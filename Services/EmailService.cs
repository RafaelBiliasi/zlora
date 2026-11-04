using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Zlora.Settings;

namespace Zlora.Services;

public class EmailService(IOptions<EmailSettings> settings)
{
    public EmailSettings Settings => settings.Value;
    public bool Configurado => !string.IsNullOrWhiteSpace(Settings.SmtpServer)
        && Settings.SenderEmail.Contains('@') && !string.IsNullOrWhiteSpace(Settings.Password)
        && !Settings.Password.StartsWith("COLOQUE_")
        && Uri.TryCreate(Settings.BaseUrl, UriKind.Absolute, out var uri)
        && (uri.Scheme == "http" || uri.Scheme == "https");

    public async Task SendEmailAsync(string destinatario, string assunto, string mensagem)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(Settings.SenderName, Settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(destinatario));
        email.Subject = assunto;
        email.Body = new TextPart("plain") { Text = mensagem };
        using var smtp = new SmtpClient { Timeout = 15000 };
        var security = Settings.Security == "SslOnConnect" ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        // Somente o capturador SMTP local de testes pode operar sem TLS.
        if (Settings.Security == "None" && new[] { "127.0.0.1", "localhost", "::1" }.Contains(Settings.SmtpServer))
            security = SecureSocketOptions.None;
        await smtp.ConnectAsync(Settings.SmtpServer, Settings.SmtpPort, security);
        await smtp.AuthenticateAsync(Settings.Username, Settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
