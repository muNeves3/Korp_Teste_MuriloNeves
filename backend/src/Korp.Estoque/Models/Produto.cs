using System.ComponentModel.DataAnnotations;

namespace Korp.Estoque.Models
{
    public class Produto
    {
        public Guid Id { get; set; }

        [Required]
        public string Codigo { get; set; } 

        [Required]  
        public string Descricao { get; set; } 

        public int Saldo { get; set;}

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
