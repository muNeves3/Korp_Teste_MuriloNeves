using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korp.Contracts
{
    public class NotaParaProcessarEvent
    {
        public Guid NotaFiscalId { get; init; }
        public List<NotaItemDto> Itens { get; init; } = new();

    }
}

public record NotaItemDto
{
    public Guid ProdutoId { get; init; }
    public int Quantidade { get; init; }
}