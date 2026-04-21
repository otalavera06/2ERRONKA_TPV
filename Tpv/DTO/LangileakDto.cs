using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.DTO
{
    public class LangileakDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public string Abizena { get; set; }
        public string Erabiltzailea { get; set; }
        public string Email { get; set; }
        public string Telefonoa { get; set; }
        public bool Baimena { get; set; }
        public int? MahaiakId { get; set; }
        public bool chatBaimena { get; set; }
    }

}
