using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class ErreserbakDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public bool Mota { get; set; }
        public int? ErabiltzaileakId { get; set; }
        public int MahaiakId { get; set; }
    }
}
