using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class EskaeraDto
    {
        public virtual int Id { get; set; }
        public virtual int ProduktuaId { get; set; }
        public virtual string Izena { get; set; }
        public virtual DateTime Data { get; set; }
        public virtual decimal Prezioa { get; set; }
        public virtual int Egoera { get; set; }
    }
}
