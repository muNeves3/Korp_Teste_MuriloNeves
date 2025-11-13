using Korp.Contracts;
using Korp.Estoque.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Consumers
{
    public class ProcessarNotaConsumer : IConsumer<NotaParaProcessarEvent>
    {
        private readonly EstoqueDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProcessarNotaConsumer(EstoqueDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<NotaParaProcessarEvent> context)
        {
            var evento = context.Message;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in evento.Itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                    if (produto == null)
                    {
                        await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                        {
                            NotaFiscalId = evento.NotaFiscalId,
                            Motivo = $"Produto ID {item.ProdutoId} não encontrado."
                        });
                        await transaction.RollbackAsync();
                        return;
                    }

                    if (produto.Saldo < item.Quantidade)
                    {
                        await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                        {
                            NotaFiscalId = evento.NotaFiscalId,
                            Motivo = $"Estoque insuficiente para o produto {produto.Descricao}."
                        });
                        await transaction.RollbackAsync();
                        return;
                    }

                    produto.Saldo -= item.Quantidade;
                }

                // Se dois consumidores tentarem alterar o mesmo produto (com RowVersion)
                // ao mesmo tempo, o EF Core detectará a colisão e lançará esta exceção.
                await _context.SaveChangesAsync();

                // Se passou, confirma a transação
                await transaction.CommitAsync();

                await _publishEndpoint.Publish(new EstoqueAtualizadoEvent
                {
                    NotaFiscalId = evento.NotaFiscalId
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();

                await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                {
                    NotaFiscalId = evento.NotaFiscalId,
                    Motivo = "Conflito de concorrência ao tentar atualizar o estoque. Tente novamente."
                });

                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                {
                    NotaFiscalId = evento.NotaFiscalId,
                    Motivo = "Erro interno no serviço de estoque: " + ex.Message
                });
            }
        }
    }
}
