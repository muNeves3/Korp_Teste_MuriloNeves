using System.ComponentModel.DataAnnotations;

namespace Korp.Estoque.DTOs
{
    public class ICreateProdutoDto
    {
        public class CreateProdutoDto
        {
            [Required(ErrorMessage = "Código obrigatório")]
            public string Codigo { get; set; }

            [Required(ErrorMessage ="Descrição obrigatória")]
            public string Descricao { get; set; }

            [Range(0, int.MaxValue, ErrorMessage ="Saldo deve ser maior que 0")]
            public int Saldo { get; set; }
        }
    }
}
