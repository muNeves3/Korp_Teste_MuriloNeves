using Korp.Contracts;
using Korp.Faturamento.Data;
using Korp.Faturamento.Enum;
using MassTransit;

namespace Korp.Faturamento.Consumers
{
    public class EstoqueAtualizadoConsumer : IConsumer<EstoqueAtualizadoEvent>
    {
        private readonly FaturamentoDbContext _context;

        public EstoqueAtualizadoConsumer(FaturamentoDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<EstoqueAtualizadoEvent> context)
        {
            var notaId = context.Message.NotaFiscalId;
            var nota = await _context.NotasFiscais.FindAsync(notaId);

            if (nota != null)
            {
                // REQUISITO: Após finalização, atualizar o status da nota para Fechada [cite: 35]
                nota.Status = StatusNotaEnum.Fechada;
                await _context.SaveChangesAsync();
            }
        }
    }
}
