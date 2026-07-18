namespace Fcg.Orders.Infrastructure.Persistence
{
    public class DatabaseSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1433;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Databasename { get; set; } = string.Empty;

        public const string SectionaName = "DatabaseSettings";
    }
}
