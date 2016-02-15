﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.Logging;
using Hangfire.MySql.Common;
using Hangfire.MySql.Entities;
using Hangfire.States;
using Hangfire.Storage;
using LinqToDB;
using LinqToDB.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Hangfire.MySql.src
{
    public class MySqlWriteOnlyTransaction : ShortConnectingDatabaseActor, IWriteOnlyTransaction
    {
        private readonly PersistentJobQueueProviderCollection _queueProviders;

        private readonly Queue<Action<DataConnection>> _commandQueue
            = new Queue<Action<DataConnection>>();

        private readonly SortedSet<string> _lockedResources = new SortedSet<string>();



        public MySqlWriteOnlyTransaction(
            string connectionString,
            PersistentJobQueueProviderCollection queueProviders) : base(connectionString)
        {
            _queueProviders = queueProviders;
            if (queueProviders == null) throw new ArgumentNullException("queueProviders");

        }

        protected ILog Logger
        {
            get { return LogProvider.GetCurrentClassLogger(); }
        }



        public void ExpireJob(string jobId, TimeSpan expireIn)
        {
            QueueCommand(db =>
            {

                db.GetTable<Entities.Job>()
                    .Where<Job>(j => j.Id == Convert.ToInt32(jobId))
                    .Set(j => j.ExpireAt, DateTime.UtcNow + expireIn)
                    .Update();
                Logger.Trace("#" + jobId + " ExpireJob in " + expireIn);
            });
        }

        private readonly DateTime? NullDateTime = null;

        public void PersistJob(string jobId)
        {

            QueueCommand(db =>
            {


                db.GetTable<Entities.Job>()
                    .Where<Job>(j => j.Id == Convert.ToInt32(jobId))
                    .Set(j => j.ExpireAt, NullDateTime)
                    .Update();

                Logger.Trace("#"+jobId+" PersistJob");
            });

        }

        public void SetJobState(string jobId, IState state)
        {
            jobId.Should().NotBeNullOrEmpty();
            state.Should().NotBeNull();

            QueueCommand(db =>
            {
                var persistedState = BuildState(jobId, state);
                var stateId = db.InsertWithIdentity(persistedState);


                db.GetTable<Job>().Where(j => j.Id == Convert.ToInt32(jobId))
                    .Set(j => j.StateId, stateId)
                    .Set(j => j.StateName, persistedState.Name)
                    .Set(j => j.StateReason, persistedState.Reason)
                    .Set(j => j.StateData, persistedState.Data)
                    .Update();

                Logger.Trace("#" + jobId + " SetJobState " + state.Name);

            });

        }

        private JobState BuildState(string jobId, IState state)
        {
            return new JobState()
            {
                JobId = Convert.ToInt32(jobId),
                CreatedAt = DateTime.UtcNow,
                Data = JsonConvert.SerializeObject(state.SerializeData()),
                Name = state.Name,
                Reason = state.Reason
            };

        }

        public void AddJobState(string jobId, IState state)
        {
            jobId.Should().NotBeNullOrEmpty();
            state.Should().NotBeNull();
            
            QueueCommand(db =>
            {
                var persistedState = BuildState(jobId, state);
                db.InsertWithIdentity(persistedState);
                Logger.Trace("#" + jobId + " AddJobState " + state.Name);
            });


        }

        public void AddToQueue(string queue, string jobId)
        {
            var provider = _queueProviders.GetProvider(queue);
            var persistentQueue = provider.GetJobQueue(ConnectionString);

            QueueCommand(_ =>
            {
                persistentQueue.Enqueue(queue, jobId);
                Logger.Trace("#" + jobId + " AddToQueue " + queue);
            });
        }


        public void IncrementCounter(string key)
        {
            QueueCommand(db => db.Insert(new Counter()
            {
                Key = key,
                Value = Counter.IncrementValue
            }));
        }


        public void IncrementCounter(string key, TimeSpan expireIn)
        {
            QueueCommand(db => db.Insert(new Counter()
            {
                Key = key,
                Value = Counter.IncrementValue,
                ExpireAt = DateTime.UtcNow + expireIn
            }));
        }

        public void DecrementCounter(string key)
        {
            QueueCommand(db => db.Insert(new Counter()
            {
                Key = key,
                Value = Counter.DecrementValue
            }));
        }

        public void DecrementCounter(string key, TimeSpan expireIn)
        {
            QueueCommand(db => db.Insert(new Counter()
            {
                Key = key,
                Value = Counter.DecrementValue,
                ExpireAt = DateTime.UtcNow + expireIn
            }));
        }

        public void AddToSet(string key, string value)
        {
            AddToSet(key, value, 0.0);
        }

        public void AddToSet(string key, string value, double score)
        {
            AcquireSetLock();

            QueueCommand(db =>
            {
                db.GetTable<Set>().Where(sv => (sv.Key == key) && (sv.Value == value)).Delete();
                db.Insert(new Set()
                {
                    Key = key,
                    Value = value,
                    Score = score
                });
            });


        }

        public void RemoveFromSet(string key, string value)
        {
            AcquireSetLock();
            QueueCommand(db => db.GetTable<Set>().Where(sv => (sv.Key == key) && (sv.Value == value)).Delete());
        }

        public void InsertToList(string key, string value)
        {
            AcquireListLock();

            QueueCommand(db => db.Insert(new List()
            {
                Key = key,
                Value = value

            }));


        }

        public void RemoveFromList(string key, string value)
        {
            AcquireListLock();

            QueueCommand(db => db.GetTable<List>().Where(list => (list.Key == key) && (list.Value == value)).Delete());

        }



        public void TrimList(string key, int keepStartingFrom, int keepEndingAt)
        {
            AcquireListLock();

            QueueCommand(db =>
            {
                var listTable = db.GetTable<List>();
                var keepIds = listTable
                    .Where(list => list.Key == key)
                    .OrderBy(list => list.Id)
                    .Skip(keepStartingFrom - 1)
                    .Take(keepEndingAt - keepStartingFrom + 1)
                    .Select(list => list.Id);

                listTable.Where(list => keepIds.Contains(list.Id)).Delete();

            });

        }

        public void SetRangeInHash(string key, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            AcquireHashLock();

            QueueCommand(db =>
            {

                foreach (var pair in keyValuePairs)
                {
                    db.GetTable<Hash>().Where(h => h.Key == key).Where(h => h.Field == pair.Key).Delete();
                    db.Insert(new Hash()
                    {
                        Key = key,
                        Field = pair.Key,
                        Value = pair.Value
                    });
                }

            });
        }

        /*
             
             const string sql = @"
;merge HangFire.Hash with (holdlock) as Target
using (VALUES (@key, @field, @value)) as Source ([Key], Field, Value)
on Target.[Key] = Source.[Key] and Target.Field = Source.Field
when matched then update set Value = Source.Value
when not matched then insert ([Key], Field, Value) values (Source.[Key], Source.Field, Source.Value);";

            AcquireHashLock();
            QueueCommand(x => x.Execute(
                sql,
                keyValuePairs.Select(y => new { key = key, field = y.Key, value = y.Value })));*/

        public void RemoveHash(string key)
        {
            AcquireHashLock();
            QueueCommand(db => db.GetTable<Hash>().Where(h => h.Key == key).Delete());
        }

        public void Commit()
        {
            Logger.Trace("Enter Commit() with " + _lockedResources.Count + " locks required");

            var timeout = TimeSpan.FromSeconds(5);
            var locks =
                _lockedResources.Select(resource => new MySqlDistributedLock(resource, timeout, ConnectionString))
                    .ToList();

            Logger.Trace("Locks established");

            try
            {
                UsingDatabase(db => { foreach (var command in _commandQueue) command(db); });
            }
            catch (Exception exception)
            {
                Logger.TraceException("Exception during Commit ", exception);
            }
            finally
            {
                Logger.Trace("Releasing locks");

                foreach (var distributedLock in locks)
                    distributedLock.Dispose();

            }

            Logger.Trace("Finished Commit()");


        }

        private void AcquireListLock()
        {
            AcquireLock(String.Format("Hangfire:List:Lock"));
        }

        private void AcquireSetLock()
        {
            AcquireLock(String.Format("Hangfire:Set:Lock"));
        }

        private void AcquireHashLock()
        {
            AcquireLock(String.Format("Hangfire:Hash:Lock"));
        }

        private void AcquireLock(string resource)
        {
            _lockedResources.Add(resource);
        }

        internal void QueueCommand(Action<DataConnection> action)
        {
            _commandQueue.Enqueue(action);
        }

        public void Dispose()
        {




            // TODO
            //throw new NotImplementedException();
        }
    }
}
