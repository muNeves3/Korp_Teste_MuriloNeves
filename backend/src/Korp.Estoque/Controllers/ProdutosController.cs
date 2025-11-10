using Korp.Estoque.Data;
using Korp.Estoque.DTOs;
using Korp.Estoque.Models;
using Korp.Estoque.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Korp.Estoque.DTOs.ICreateProdutoDto;

namespace Korp.Estoque.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly EstoqueDbContext _context;

        public ProdutosController(EstoqueDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IProdutoDto>>> GetProdutos()
        {
            var produtos = await _context.Produtos
                .Select(p => new IProdutoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IProdutoDto>> GetProduto(Guid id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return new ProdutoNaoEncontradoError();
            }

            var produtoDto = new IProdutoDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Saldo = produto.Saldo
            };

            return Ok(produtoDto);
        }

        [HttpPost]
        public async Task<ActionResult<IProdutoDto>> CreateProduto(CreateProdutoDto createDto)
        {
            var produto = new Produto
            {
                Id = Guid.NewGuid(),
                Codigo = createDto.Codigo,
                Descricao = createDto.Descricao,
                Saldo = createDto.Saldo
            };

            await _context.Produtos.AddAsync(produto);
            await _context.SaveChangesAsync();

            var produtoDto = new IProdutoDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Saldo = produto.Saldo
            };

            return CreatedAtAction(nameof(GetProduto), new { id = produtoDto.Id }, produtoDto);
        }
    }
}
