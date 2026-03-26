using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class ErreserbakSortuDto
    {
        public DateTime Data {  get; set; }
        public bool Mota {  get; set; }
        public int? ErabiltzaileakId { get; set; }
        public int MahaiakId { get; set; }
    }
}
