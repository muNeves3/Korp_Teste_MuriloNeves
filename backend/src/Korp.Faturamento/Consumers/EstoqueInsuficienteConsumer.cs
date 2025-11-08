using Korp.Contracts;
using Korp.Faturamento.Data;
using Korp.Faturamento.Enum;
using MassTransit;

namespace Korp.Faturamento.Consumers
{
    public class EstoqueInsuficienteConsumer : IConsumer<EstoqueInsuficienteEvent>
    {
        private readonly FaturamentoDbContext _context;
        private readonly ILogger<EstoqueInsuficienteConsumer> _logger;

        public EstoqueInsuficienteConsumer(FaturamentoDbContext context, ILogger<EstoqueInsuficienteConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EstoqueInsuficienteEvent> context)
        {
            var notaId = context.Message.NotaFiscalId;
            var motivo = context.Message.Motivo;

            _logger.LogWarning("Falha ao processar Nota Fiscal {NotaId}: {Motivo}", notaId, motivo);

            var nota = await _context.NotasFiscais.FindAsync(notaId);

            if (nota != null)
            {
                // A nota falhou. Damos um status de Rejeitada
                // (ou poderíamos reverter para "Aberta" para o usuário corrigir)
                nota.Status = StatusNotaEnum.Rejeitada;
                await _context.SaveChangesAsync();
            }
        }
    }
}
