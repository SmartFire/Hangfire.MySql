using System;
using System.Configuration;
using Hangfire.MySql.src;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql.Common
{
    public class ShortConnectingDatabaseActor : DatabaseActor
    {
        private readonly string _connectionString;

        public ShortConnectingDatabaseActor(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected string ConnectionString { get { return _connectionString; } }

        protected override TResult Invoke<TResult>(Func<DataConnection, TResult> func)
        {
                using (var dc = new DataConnection(new MySqlDataProvider(),_connectionString))
                {
                    return func(dc);
                }
            
        }


        private bool IsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.Contains(";");
        }

        private bool IsConnectionStringInConfiguration(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionStringSetting != null;
        }
    }
}
