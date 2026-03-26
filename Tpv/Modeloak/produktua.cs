﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpv.Modeloak
{
    public class Produktua
    {
        public virtual int Id { get; set; }
        public virtual string Izena { get; set; }
        public virtual decimal Prezioa { get; set; }
        public virtual string Irudia { get; set; }
        public virtual int ProduktuenMotakId { get; set; }
    }
}
