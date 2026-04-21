using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class EskaerakSortuDto
    {
        public int ProduktuaId { get; set; }
        public string Izena { get; set; }
        public decimal Prezioa { get; set; }
        public DateTime Data { get; set; }
        public int Egoera { get; set; }
        public bool IsPlatera { get; set; }
    }
}
