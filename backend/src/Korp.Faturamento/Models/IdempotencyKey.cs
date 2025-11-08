using System.ComponentModel.DataAnnotations;

namespace Korp.Faturamento.Models
{
    public class IdempotencyKey
    {
        [Key]
        public Guid Key { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

