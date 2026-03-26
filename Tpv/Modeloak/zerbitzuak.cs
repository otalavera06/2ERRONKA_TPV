using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.Modeloak
{
    public class zerbitzuak
    {
        public virtual int Id { get; set; }
        public virtual decimal PrezioTotala { get; set; }
        public virtual DateTime Data { get; set; }
        public virtual int? ErreserbaId { get; set; }
        public virtual int? MahaiakId { get; set; }

        public virtual IList<eskaerak> Eskaerak { get; set; } = new List<eskaerak>();

    }
}
