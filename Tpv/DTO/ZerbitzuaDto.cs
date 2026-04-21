using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpv.Modeloak;

namespace Tpv.DTO
{
    public class ZerbitzuaDto
    {
        public virtual int Id { get; set; }
        public virtual decimal PrezioTotala { get; set; }
        public virtual DateTime Data { get; set; }
        public virtual int? ErreserbaId { get; set; }
        public virtual int? MahaiakId { get; set; }
        public virtual bool Ordainduta { get; set; }

        public virtual IList<EskaeraDto> Eskaerak { get; set; } = new List<EskaeraDto>();
        public string ZerbitzuaHeader => $"#{Id} - {Data} - {PrezioTotala}€";
    }
}
