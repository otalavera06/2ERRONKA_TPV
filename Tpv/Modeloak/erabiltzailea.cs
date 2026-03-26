using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.Modeloak
{
    using System;

    namespace Tpv.Modeloak
    {
        public class Erabiltzailea
        {
            public virtual int Id { get; set; }
            public virtual string erabiltzailea { get; set; }
            public virtual string pasahitza { get; set; }
            public virtual bool baimena { get; }
         
        }
    }

}
