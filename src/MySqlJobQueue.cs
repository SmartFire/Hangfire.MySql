﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.MySql.Common;
using Hangfire.MySql.Entities;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql.src
{
    internal class MySqlJobQueue : ShortConnectingDatabaseActor, IPersistentJobQueue
    {
        private readonly MySqlStorageOptions _options;

        public MySqlJobQueue(string connectionString, MySqlStorageOptions options)
            : base(connectionString)
        {
            _options = options;
        }

        protected ILog Logger
        {
            get { return LogProvider.GetCurrentClassLogger(); }
        }



        // why did Frank use his own attributes ??

        [NotNull]
        public IFetchedJob Dequeue(string[] queues, CancellationToken cancellationToken)
        {

            Logger.Trace(DateTime.Now.ToLongTimeString() + " enter Dequeue()");

            MySqlFetchedJob job = null;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                UsingDatabase(db =>
                {

                    string token = Guid.NewGuid().ToString();
                    int nUpdated = db.GetTable<JobQueue>(true)
                        .Where(jq => jq.FetchedAt == NullDateTime)
                        .Where(jq => queues.Contains(jq.Queue))
                        .Take(1)
                        .Set(jq => jq.FetchedAt, DateTime.UtcNow)
                        .Set(jq => jq.FetchToken, token)
                        .Update();

                    if (nUpdated != 0)
                    {

                        nUpdated.Should().Be(1);
                        var jobQueue = db.GetTable<JobQueue>(true).Single(jq => jq.FetchToken == token);

                        Logger.Trace(DateTime.Now.ToLongTimeString() + " returning fetched job " + jobQueue.JobId);

                        job = new MySqlFetchedJob(ConnectionString,
                            jobQueue.Id,
                            jobQueue.JobId.ToString(CultureInfo.InvariantCulture),
                            jobQueue.Queue);
                    }


                    Logger.Trace(DateTime.Now.ToLongTimeString() + " waiting");


                    cancellationToken.WaitHandle.WaitOne(_options.QueuePollInterval);
                    cancellationToken.ThrowIfCancellationRequested();


                });
            } while (job == null);

            return job;

        }

        public void Enqueue(string queue, string jobId)
        {

            queue.Should().NotBeNullOrEmpty();
            jobId.Should().NotBeNullOrEmpty();

            UsingDatabase(db =>
            {
                try
                {

                    {
                        db.Insert(new Entities.JobQueue()
                        {
                            JobId = Convert.ToInt32(jobId),
                            Queue = queue
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });

        }
    }

}