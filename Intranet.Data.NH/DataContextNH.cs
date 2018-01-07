using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Intranet.Data.NH
{
    public static class DataContextNH
    {
        #region Private
        private static Configuration  _configuration;
        private static ISessionFactory _sessionFactory;
        
        private static ISessionFactory CreateSessionFactory()
        {
            ISessionFactory session = null;
            try
            {
                session = Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("Intranet")))
                                  .Mappings(m =>
                                      {
                                          m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly());
                                      })
                                  .ExposeConfiguration(Cfg => _configuration = Cfg)
                                  .BuildSessionFactory();
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
                throw;
            }
            return session;
        }
        private static void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            //if (File.Exists(DbFile))
            //    File.Delete(DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            //new SchemaExport(config)
            //    .Create(false, true);
        }
        #endregion

        #region Public
        static DataContextNH()
        {
            _sessionFactory = CreateSessionFactory();
        }
        public static Configuration Configuration
        {
            get { return _configuration; }
        }
        public static ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }
        public static ISession OpenSession()
        {
            ISession session = _sessionFactory.OpenSession();
            return session;
        }
        #endregion
    }
}