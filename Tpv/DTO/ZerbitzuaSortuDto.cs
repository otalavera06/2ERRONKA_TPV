using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class ZerbitzuaSortuDto
    {
        public DateTime Data { get; set; }
        public int? ErreserbaId { get; set; }
        public int? MahaiakId { get; set; }
        public decimal PrezioTotala { get; set; }
        public IList<EskaerakSortuDto> Eskaerak { get; set; } = new List<EskaerakSortuDto>();
    }
}
