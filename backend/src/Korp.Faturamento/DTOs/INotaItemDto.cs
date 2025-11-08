namespace Korp.Faturamento.DTOs
{
    public class INotaItemDto
    {
        public Guid Id { get; set; }
        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
