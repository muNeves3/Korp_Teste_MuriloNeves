namespace Korp.Faturamento.Models
{
    public class NotaFiscalItem
    {
        public Guid Id { get; set; }

        public Guid NotaFiscalId { get; set; }
        public NotaFiscal NotaFiscal { get; set; }

        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; } 
    }
}
