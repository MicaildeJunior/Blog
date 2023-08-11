namespace Blog;

public static class Configuration
{
    public static string JwtKey = "f5b2f809728a4773b6681d9515840d53";
    public static string ApiKeyName = "api_key";
    public static string ApiKey = "curso_api_IlTevUZ/z=ey3NwwCV/ynWg$=";
    public static SmtpConfiguration Smtp = new();

    public class SmtpConfiguration
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 25;
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}