﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Tpv.Modeloak;
using FluentNHibernate.Mapping;

namespace Tpv.Mapeoak
{
    public class ProduktuakMap : ClassMap<Produktua>
    {
        public ProduktuakMap()
        {
            Table("produktuak");
            Id(x => x.Id).Column("id").GeneratedBy.Identity();
            Map(x => x.Izena).Column("izena");
            Map(x => x.Prezioa).Column("prezioa");
            Map(x => x.Irudia).Column("irudia");
            Map(x => x.ProduktuenMotakId).Column("produktuen_motak_id");
        }
    }
}
