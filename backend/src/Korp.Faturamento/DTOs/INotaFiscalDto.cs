namespace Korp.Faturamento.DTOs
{
    public class INotaFiscalDto
    {
        public Guid Id { get; set; }
        public int NumeroSequencial { get; set; }
        public string Status { get; set; }
        public List<INotaItemDto> Itens { get; set; } = new();
    }
}
