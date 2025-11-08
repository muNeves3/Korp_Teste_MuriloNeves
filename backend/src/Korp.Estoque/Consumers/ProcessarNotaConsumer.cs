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

            // Usamos uma transação para garantir que a baixa de todos os itens
            // da nota funcione, ou nenhum funcione.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in evento.Itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                    if (produto == null)
                    {
                        // Se o produto não existe, falha e avisa o Faturamento
                        await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                        {
                            NotaFiscalId = evento.NotaFiscalId,
                            Motivo = $"Produto ID {item.ProdutoId} não encontrado."
                        });
                        await transaction.RollbackAsync();
                        return; // Para a execução
                    }

                    if (produto.Saldo < item.Quantidade)
                    {
                        // Se o saldo é insuficiente, falha e avisa o Faturamento
                        await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                        {
                            NotaFiscalId = evento.NotaFiscalId,
                            Motivo = $"Estoque insuficiente para o produto {produto.Descricao}."
                        });
                        await transaction.RollbackAsync();
                        return; // Para a execução
                    }

                    // Se tudo estiver OK, dá a baixa no saldo
                    produto.Saldo -= item.Quantidade;
                }

               // --- TRATAMENTO DE CONCORRÊNCIA  ---
                            // O SaveChangesAsync() é onde a mágica acontece.
                            // Se dois consumidores tentarem alterar o MESMO produto (com RowVersion)
                            // ao mesmo tempo, o EF Core detectará a colisão e lançará esta exceção.
                await _context.SaveChangesAsync();

                // Se passou, confirma a transação
                await transaction.CommitAsync();

                // REQUISITO: Atualizar o saldo dos produtos [cite: 37]
                            // E avisa o Faturamento que deu tudo certo
                await _publishEndpoint.Publish(new EstoqueAtualizadoEvent
                {
                    NotaFiscalId = evento.NotaFiscalId
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // --- CONCORRÊNCIA DETECTADA! ---
                await transaction.RollbackAsync();

                // Avisa o Faturamento que falhou (ex: "Conflito de concorrência")
                await _publishEndpoint.Publish(new EstoqueInsuficienteEvent
                {
                    NotaFiscalId = evento.NotaFiscalId,
                    Motivo = "Conflito de concorrência ao tentar atualizar o estoque. Tente novamente."
                });

                // Importante: Lança a exceção de volta para o MassTransit
                // Isso fará o MassTransit tentar reprocessar a mensagem
                // (após algumas tentativas, ela irá para uma fila de erro)
                throw;
            }
            catch (Exception ex)
            {
                // Erro genérico
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
