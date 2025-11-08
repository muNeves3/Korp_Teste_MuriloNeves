using System.ComponentModel.DataAnnotations;

namespace Korp.Faturamento.DTOs
{
    public class ICreateNotaFiscalDto
    {
        [Required]
        [MinLength(1)]
        public List<ICreateNotaItemDto> Itens { get; set; } = new();
    }
}
