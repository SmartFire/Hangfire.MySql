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
        private MySqlJobQueue _jobQueue;
        private MySqlJobQueueMonitoringApi _monitoringApi;

        public MySqlJobQueueProvider(string connectionString, MySqlStorageOptions options)
            : base(connectionString)
        {
            options.Should().NotBeNull();
            _options = options;

            _jobQueue = new MySqlJobQueue(connectionString, _options);
            _monitoringApi = new MySqlJobQueueMonitoringApi(connectionString, _options);
        }

        protected ILog Logger
        {
            get { return LogProvider.GetCurrentClassLogger(); }
        }




        public IPersistentJobQueue GetJobQueue(string connectionString)
        {
            Logger.Trace(DateTime.Now.ToLongTimeString() + " GetJobQueue ");
            return _jobQueue;
        }



        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi(string connectionString)
        {
            Logger.Trace(DateTime.Now.ToLongTimeString() + " GetJobQueueMonitoringApi ");
            return _monitoringApi;
        }

      
    }
}
