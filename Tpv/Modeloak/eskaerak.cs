using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.Modeloak
{
    public class eskaerak
    {
        public virtual int Id { get; set; }
        public virtual string izena { get; set; }
        public virtual DateTime data { get; set; }
        public virtual decimal prezioa { get; set; }
        public virtual int egoera { get; set; }
        public virtual int? zerbitzua_id { get; set; }
        public virtual int produktua_id { get; set; }
    }
}
