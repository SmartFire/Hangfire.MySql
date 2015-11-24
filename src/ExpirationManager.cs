using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire.Logging;
using Hangfire.MySql.Common;
using Hangfire.Server;
using LinqToDB.Data;

namespace Hangfire.MySql.src
{
    internal class ExpirationManager : IServerComponent
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private const string DistributedLockKey = "locks:expirationmanager";
        private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DelayBetweenPasses = TimeSpan.FromSeconds(1);
        private const int NumberOfRecordsInSinglePass = 1000;

        private static readonly string[] ProcessedTables =
        {
            "AggregatedCounter",
            "Job",
            "List",
            "Set",
            "Hash",
        };

        private readonly MySqlStorage _storage;
        private readonly TimeSpan _checkInterval;

        public ExpirationManager(MySqlStorage storage)
            : this(storage, TimeSpan.FromHours(1))
        {
        }

        public ExpirationManager(MySqlStorage storage, TimeSpan checkInterval)
        {
            if (storage == null) throw new ArgumentNullException("storage");

            _storage = storage;
            _checkInterval = checkInterval;
        }

        public void Execute(CancellationToken cancellationToken)
        {
            foreach (var table in ProcessedTables)
            {
                Logger.DebugFormat("Removing outdated records from table '{0}'...", table);

                int removedCount = 0;

                do
                {
                    _storage.UsingDatabase(connection =>
                    {
                        using (
                            var lck = _storage.GetConnection()
                                .AcquireDistributedLock(DistributedLockKey, DefaultLockTimeout))
                        {

                            removedCount = connection.Execute(
                                String.Format(@"
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
START TRANSACTION;
delete from [{0}].[{1}] where ExpireAt < @now limit @count;
COMMIT;", connection.DataProvider.GetSchemaProvider().GetSchema(connection).Database, table),
                                new { now = DateTime.UtcNow, count = NumberOfRecordsInSinglePass });

                        }
                    });

                    if (removedCount > 0)
                    {
                        Logger.Trace(String.Format("Removed {0} outdated record(s) from '{1}' table.", removedCount,
                            table));

                        cancellationToken.WaitHandle.WaitOne(DelayBetweenPasses);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                } while (removedCount != 0);
            }

            cancellationToken.WaitHandle.WaitOne(_checkInterval);
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }

}