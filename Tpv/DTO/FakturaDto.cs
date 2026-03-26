using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class FakturaDto
    {
        public int Id { get; set; }
        public int ZerbitzuaId { get; set; }
        public decimal PrezioTotala { get; set; }
        public bool Sortuta { get; set; }
    }
}
