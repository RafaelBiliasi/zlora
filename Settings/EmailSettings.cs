namespace Zlora.Settings;

// Preencha EmailSettings em appsettings.json, como no projeto VasosInteligentes.
public class EmailSettings
{
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string Security { get; set; } = "StartTls";
    public string SenderName { get; set; } = "Zlora";
    public string SenderEmail { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string BaseUrl { get; set; } = "http://localhost:5080";
}
