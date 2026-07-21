using System.ComponentModel.DataAnnotations;

namespace Fcg.Orders.Infrastructure.Caching
{
    public class RedisSettings
    {
        [Required(ErrorMessage = "O Host do Redis é obrigatório.")]
        public string Host { get; set; } = string.Empty;
        [Range(1, 65535, ErrorMessage = "A porta do Redis deve ser válida (1 a 65535).")]
        public int Port { get; set; } = 6379;
        public string Password { get; set; } = string.Empty;
        public string InstanceName { get; set; }

        public RedisSettings() { }

        public const string RedisSectionName = "RedisSettings";
    }
}
