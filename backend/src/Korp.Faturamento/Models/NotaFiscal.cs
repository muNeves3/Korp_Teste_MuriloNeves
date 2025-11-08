using Korp.Faturamento.Enum;

namespace Korp.Faturamento.Models
{
    public class NotaFiscal
    {
        public Guid Id { get; set; }
        public int NumeroSequencial { get; set; }
        public StatusNotaEnum Status { get; set; } 

        public ICollection<NotaFiscalItem> Itens { get; set; } = new List<NotaFiscalItem>();
    }
}
