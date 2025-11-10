using System.ComponentModel.DataAnnotations;

namespace Korp.Faturamento.DTOs
{
    public class ICreateNotaFiscalDto
    {
        [Required(ErrorMessage ="É necessário ter pelo menos 1 (um) item")]
        [MinLength(1)]
        public List<ICreateNotaItemDto> Itens { get; set; } = new();
    }
}
