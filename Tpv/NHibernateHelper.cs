﻿using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpv.Mapeoak;

namespace Tpv
{
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                    InitializeSessionFactory();

                return _sessionFactory;
            }
        }

        private static void InitializeSessionFactory()
        {
            _sessionFactory = Fluently.Configure()
                .Database(
                    MySQLConfiguration.Standard
                        .ConnectionString(
                                "Server=localhost;Port=3306;Database=erronka2_2026;Uid=root;Pwd=1mg2024;"
                        //"Server=192.168.1.104;Port=3306;Database=erronka2_2026;Uid=taldea5;Pwd=taldea5;"
                        //"Server=192.168.115.159;Port=3306;Database=erronka2_2026;Uid=taldea5;Pwd=taldea5;"


                        )
                )
                .Mappings(m =>
                    m.FluentMappings.AddFromAssemblyOf<ErabiltzaileaMap>())
                    .Mappings(m =>
                    m.FluentMappings.AddFromAssemblyOf<ProduktuakMap>())
                .BuildSessionFactory();
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
