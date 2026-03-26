using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpv.Modeloak;

namespace Tpv.Mapeoak
{
    public class EskaeraMap: ClassMap<eskaerak>
    {
        public EskaeraMap()
        {
            
            Table("eskaerak");

            Id(x => x.Id).GeneratedBy.Identity().Column("id");

            Map(x => x.izena).Column("izena").Length(100).Nullable();
            Map(x => x.prezioa).Column("prezioa").Not.Nullable();

            Map(x => x.data)
                .Column("data")
                .Not.Nullable();

            Map(x => x.egoera)
                .Column("egoera")
                .Not.Nullable();
            Map(x => x.zerbitzua_id)
                .Column("zerbitzua_id").Nullable();

            Map(x => x.produktua_id)
                .Column("produktua_id")
                .Not.Nullable();
        }
    }
}

