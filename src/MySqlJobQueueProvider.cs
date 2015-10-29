using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.Logging;
using Hangfire.MySql.Common;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql.src
{
    public class MySqlJobQueueProvider : ShortConnectingDatabaseActor, IPersistentJobQueueProvider
    {
        private readonly MySqlStorageOptions _options;

        public MySqlJobQueueProvider(string connectionString, MySqlStorageOptions options)
            : base(connectionString)
        {
            options.Should().NotBeNull();
            _options = options;
        }

        protected ILog Logger
        {
            get { return LogProvider.GetCurrentClassLogger(); }
        }




        public IPersistentJobQueue GetJobQueue(string connectionString)
        {
            Logger.Trace(DateTime.Now.ToLongTimeString() + " GetJobQueue ");


            return new MySqlJobQueue(connectionString, _options);
        }



        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi(string connectionString)
        {
            Logger.Trace(DateTime.Now.ToLongTimeString() + " GetJobQueueMonitoringApi ");

            return new MySqlJobQueueMonitoringApi(connectionString, _options);
        }

      
    }
}
