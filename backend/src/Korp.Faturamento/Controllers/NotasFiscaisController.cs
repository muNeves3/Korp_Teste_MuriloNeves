using Korp.Faturamento.Data;
using Korp.Faturamento.DTOs;
using Korp.Faturamento.Enum;
using Korp.Faturamento.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Korp.Contracts;

namespace Korp.Faturamento.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly FaturamentoDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public NotasFiscaisController(FaturamentoDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<INotaFiscalDto>>> GetNotasFiscais()
        {
            var notas = await _context.NotasFiscais
                .Include(n => n.Itens)
                .Select(n => new INotaFiscalDto
                {
                    Id = n.Id,
                    NumeroSequencial = n.NumeroSequencial,
                    Status = n.Status.ToString(),
                    Itens = n.Itens.Select(i => new INotaItemDto
                    {
                        Id = i.Id,
                        ProdutoId = i.ProdutoId,
                        Quantidade = i.Quantidade
                    }).ToList()
                })
                .OrderBy(n => n.NumeroSequencial)
                .ToListAsync();

            return Ok(notas);
        }

        [HttpPost]
        public async Task<ActionResult<INotaFiscalDto>> CreateNotaFiscal(ICreateNotaFiscalDto createDto)
        {
            var ultimoNumero = await _context.NotasFiscais
                .OrderByDescending(n => n.NumeroSequencial)
                .Select(n => n.NumeroSequencial)
                .FirstOrDefaultAsync();

            var novaNota = new NotaFiscal
            {
                Id = Guid.NewGuid(),
                NumeroSequencial = ultimoNumero + 1,
                Status = StatusNotaEnum.Aberta
            };

            foreach (var itemDto in createDto.Itens)
            {
                novaNota.Itens.Add(new NotaFiscalItem
                {
                    Id = Guid.NewGuid(),
                    ProdutoId = itemDto.ProdutoId,
                    Quantidade = itemDto.Quantidade
                });
            }

            await _context.NotasFiscais.AddAsync(novaNota);
            await _context.SaveChangesAsync();

            var notaDto = new INotaFiscalDto
            {
                Id = novaNota.Id,
                NumeroSequencial = novaNota.NumeroSequencial,
                Status = novaNota.Status.ToString(),
                Itens = novaNota.Itens.Select(i => new INotaItemDto
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            return CreatedAtAction(nameof(GetNotasFiscais), new { id = notaDto.Id }, notaDto);
        }

        [HttpPost("{id}/imprimir")]
        public async Task<IActionResult> ImprimirNota(Guid id,
        [FromHeader(Name = "X-Idempotency-Key")] Guid? idempotencyKey)
        {
            if (idempotencyKey == null || idempotencyKey == Guid.Empty)
            {
                return BadRequest(new { message = "O header 'X-Idempotency-Key' é obrigatório." });
            }

            // Verifica se essa chave já foi processada.
            // Se sim, apenas retorne OK (a requisição foi um sucesso idempotente).
            var jaProcessado = await _context.IdempotencyKeys
                .AnyAsync(k => k.Key == idempotencyKey.Value);

            if (jaProcessado)
            {
                return Ok(new { message = "Requisição já processada." });
            }

            // --- 2. VALIDAÇÃO DE NEGÓCIO ---
            var nota = await _context.NotasFiscais
                .Include(n => n.Itens) // Importante: carregar os itens da nota
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
            {
                return NotFound(new { message = "Nota Fiscal não encontrada." });
            }

            // REQUISITO: Não permitir a impressão de notas com status diferente de Aberta [cite: 36]
            if (nota.Status != StatusNotaEnum.Aberta)
            {
                return BadRequest(new { message = "Nota não pode ser impressa (status atual: " + nota.Status + ")." });
            }

            // --- 3. PROCESSAMENTO E TRANSAÇÃO ---
            // Usamos uma transação para garantir que a Chave de Idempotência
            // e a mudança de Status da Nota sejam salvas juntas (atomicamente).
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // A. Salva a chave de idempotência
                await _context.IdempotencyKeys.AddAsync(new IdempotencyKey { Key = idempotencyKey.Value });

                // B. Atualiza o status da nota para "Processando"
                // Isso cumpre o requisito de "exibir indicador de processamento" [cite: 34]
                nota.Status = StatusNotaEnum.Processando;

                // Salva as duas mudanças no banco
                await _context.SaveChangesAsync();

                // C. Prepara a mensagem para o RabbitMQ (Serviço de Estoque)
                var evento = new NotaParaProcessarEvent
                {
                    NotaFiscalId = nota.Id,
                    Itens = nota.Itens.Select(item => new NotaItemDto
                    {
                        ProdutoId = item.ProdutoId,
                        Quantidade = item.Quantidade
                    }).ToList()
                };

                // D. Publica a mensagem na fila
                // O MassTransit garante que isso será entregue
                await _publishEndpoint.Publish(evento);

                // E. Confirma a transação
                await transaction.CommitAsync();

                // Retorna 202 Accepted: "Sua requisição foi aceita
                // e está sendo processada em segundo plano."
                return Accepted(new { message = "Nota enviada para processamento." });
            }
            catch (DbUpdateException ex)
            {
                // Erro de concorrência: Duas requisições com a MESMA chave de idempotência
                // chegaram ao "mesmo tempo". A primeira passou, a segunda falhou no
                // "AddAsync(new IdempotencyKey...)" por violar a Primary Key.
                // Isso é o esperado.
                await transaction.RollbackAsync();
                return Ok(new { message = "Requisição duplicada detectada e ignorada." });
            }
            catch (Exception ex)
            {
                // Se qualquer outra coisa der errado, desfaz tudo.
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Erro interno: " + ex.Message });
            }
        }
    }
}
