using System.ComponentModel.DataAnnotations;

namespace Korp.Faturamento.DTOs
{
    public class ICreateNotaItemDto
    {
        [Required]
        public Guid ProdutoId { get; set; } 

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }
    }
}
