using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korp.Contracts
{
    public class EstoqueInsuficienteEvent
    {
        public Guid NotaFiscalId { get; init; }
        public string Motivo { get; init; }
    }
}
