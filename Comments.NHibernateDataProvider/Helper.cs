﻿using NHibernate;
using NHibernate.Cfg;

namespace Comments.NHibernateDataProvider
{
    class Helper
    {
        private static ISessionFactory sessionFactory;

        public static ISession OpenSession()
        {
            if (sessionFactory == null)
            {
                var config = new Configuration();
                var data = config.Configure();
                config.AddAssembly(typeof(Helper).Assembly);
                sessionFactory = data.BuildSessionFactory();
            }

            return sessionFactory.OpenSession();
        }
    }
}
