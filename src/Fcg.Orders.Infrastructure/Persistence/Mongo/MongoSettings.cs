using System;
using System.ComponentModel.DataAnnotations;

namespace Fcg.Orders.Infrastructure.Persistence.Mongo
{
    internal class MongoSettings
    {
        public const string SectionName = "MongoDatabaseSettings";
        [Required(ErrorMessage = "Host do banco de dados é obrigatório.")]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "Porta do banco de dados deve estar entre 1 e 65535.")]
        public int Port { get; set; } = 27017;

        [Required(ErrorMessage = "Nome de usuário é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome do banco de dados é obrigatório.")]
        public string DatabaseName { get; set; } = string.Empty;

        public string ConnectionString => $"mongodb://{Uri.EscapeDataString(Username)}:{Uri.EscapeDataString(Password)}@{Host}:{Port}/{DatabaseName}?authSource=admin";
    }
}
