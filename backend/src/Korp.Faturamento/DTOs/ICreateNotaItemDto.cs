using System.ComponentModel.DataAnnotations;

namespace Korp.Faturamento.DTOs
{
    public class ICreateNotaItemDto
    {
        [Required(ErrorMessage ="ProdutoId é obrigatório")]
        public Guid ProdutoId { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage ="Quantidade deve ser maior que 1")]
        public int Quantidade { get; set; }
    }
}
