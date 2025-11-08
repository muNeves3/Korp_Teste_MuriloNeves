namespace Korp.Estoque.DTOs
{
    public class IProdutoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public int Saldo { get; set; }
    }
}
