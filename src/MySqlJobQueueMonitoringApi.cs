using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.MySql.Common;
using Hangfire.MySql.Entities;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql.src
{
    public class MySqlJobQueueMonitoringApi : ShortConnectingDatabaseActor, IPersistentJobQueueMonitoringApi
    {
        private readonly MySqlStorageOptions _options;

        public MySqlJobQueueMonitoringApi(string connectionString, MySqlStorageOptions options)
            : base(connectionString)
        {
            options.Should().NotBeNull();
            _options = options;
        }

        public IEnumerable<string> GetQueues()
        {
            return
                UsingTable<Entities.JobQueue, IEnumerable<string>>(
                    jobQueues => jobQueues.Select(jq => jq.Queue).Distinct());
        }

        public IEnumerable<int> GetEnqueuedJobIds(string queue, int @from, int perPage)
        {
            return
                UsingTable<Entities.JobQueue, IEnumerable<int>>(
                    jobQueues => jobQueues
                                .InQueue(queue)
                                .OrderBy(jobQueue => jobQueue.Id)
                                .Skip(from)
                                .Take(perPage)
                                .Select(jobQueue => jobQueue.JobId));
        }

        public IEnumerable<int> GetFetchedJobIds(string queue, int @from, int perPage)
        {
            return Enumerable.Empty<int>();
        }

        public EnqueuedAndFetchedCount GetEnqueuedAndFetchedCount(string queue)
        {
            var enqueuedAndFetchedCount = new EnqueuedAndFetchedCount()
            {
                EnqueuedCount = UsingTable<Entities.JobQueue, long>(jobQueues => jobQueues.InQueue(queue).Count()),
                FetchedCount = 0
            };

            return enqueuedAndFetchedCount;
        }

    }
}

