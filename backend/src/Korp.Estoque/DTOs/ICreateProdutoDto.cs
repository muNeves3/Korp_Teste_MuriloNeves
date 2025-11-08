using System.ComponentModel.DataAnnotations;

namespace Korp.Estoque.DTOs
{
    public class ICreateProdutoDto
    {
        public class CreateProdutoDto
        {
            [Required]
            public string Codigo { get; set; }

            [Required]
            public string Descricao { get; set; }

            [Range(0, int.MaxValue)]
            public int Saldo { get; set; }
        }
    }
}
