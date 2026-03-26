using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using FluentNHibernate.Mapping;
using Tpv.Modeloak;

namespace Tpv.Mapeoak
{
    public class zerbitzuakmap : ClassMap<zerbitzuak>
    {
        public zerbitzuakmap()
        {
            Table("zerbitzua"); 
            Id(x => x.Id).GeneratedBy.Identity().Column("id");
            Map(x => x.PrezioTotala).Column("prezioTotala").Not.Nullable();
            Map(x => x.Data).Column("data").Not.Nullable();
            Map(x => x.ErreserbaId).Column("erreserba_id").Nullable();
            Map(x => x.MahaiakId).Column("mahaiak_id").Nullable();

            HasMany<eskaerak>(x => x.Eskaerak).KeyColumn("zerbitzua_id").Inverse().Cascade.All();
        }
    }
}
