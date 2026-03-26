using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpv.Modeloak;
using Tpv.Modeloak.Tpv.Modeloak;

namespace Tpv.Mapeoak
{
    public class ErabiltzaileaMap : ClassMap<Erabiltzailea>
    {
        public ErabiltzaileaMap()
        {
            Table("langileak");

            
            Id(x => x.Id)
                .Column("id")
                .GeneratedBy.Identity();

            Map(x => x.erabiltzailea)
                .Column("erabiltzailea")
                .Length(50)
                .Not.Nullable();

            Map(x => x.pasahitza)
                .Column("pasahitza")
                .Length(50)
                .Not.Nullable();
            Map(x => x.baimena)
                .Column("baimena")
                .Not.Nullable();
        }
    }
}