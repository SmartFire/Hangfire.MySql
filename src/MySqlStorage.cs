﻿using System;
using System.Collections.Generic;
using System.Configuration;
using FluentAssertions;
using Hangfire.Annotations;
using Hangfire.MySql.src;
using Hangfire.Server;
using Hangfire.MySql.Common;
using Hangfire.Storage;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using LinqToDB.SchemaProvider;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql
{
    public class MySqlStorage : JobStorage, IDisposable
    {
        private readonly string _connectionString;
        private readonly MySqlStorageOptions _options;
     

      


        public MySqlStorage(string nameOrConnectionString)
            : this(nameOrConnectionString, new MySqlStorageOptions())
        {
        }

        

        public MySqlStorage(string nameOrConnectionString, MySqlStorageOptions options)
        {
            _connectionString = GetConnectionStringFrom(nameOrConnectionString);
            _options = options;
            
            Setup setup = new Setup(_connectionString);
            if(options.EnsureDatabase)
                setup.EnsureDatabase();

            InitializeQueueProviders();



        }

        public PersistentJobQueueProviderCollection QueueProviders { get; private set; }


        public override IMonitoringApi GetMonitoringApi()
        {
            return new MySqlMonitoringApi(_connectionString, QueueProviders);
        }

        public override IStorageConnection GetConnection()
        {
            return new MySqlStorageConnection(_connectionString, QueueProviders);
        }

        public override IEnumerable<IServerComponent> GetComponents()
        {
            yield return new ExpirationManager(this, TimeSpan.FromHours(1));  // CONFIG !
           // yield return new CountersAggregator(this, _options.CountersAggregateInterval);
        }

        private TResult UseConnection<TResult>(Func<DataConnection, TResult> func)
        {
            using (var dc = new DataConnection(DatabaseActor.DataProvider, _connectionString))
            {
                return func(dc);
            }
        }

        public void UsingDatabase(Action<DataConnection> action)
        {
            UseConnection(dc => { action(dc); return true; });
        }

        private void InitializeQueueProviders()
        {
            var defaultQueueProvider = new MySqlJobQueueProvider(_connectionString,_options);
            QueueProviders = new PersistentJobQueueProviderCollection(defaultQueueProvider);
        }


        private string GetConnectionStringFrom(string nameOrConnectionString)
        {
            nameOrConnectionString.Should().NotBeNullOrEmpty();

            if (nameOrConnectionString.Contains(";"))
                return nameOrConnectionString;


            ConfigurationManager.ConnectionStrings[nameOrConnectionString].Should().NotBeNull("Connection string name "
                                                                                              + nameOrConnectionString +
                                                                                              " not found.");

            return ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;


        }


        public void Dispose()
        {
        }
    }

    public static class MySqlStorageExtensions
    {
        public static IGlobalConfiguration<MySqlStorage> UseMySqlStorage(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] string nameOrConnectionString)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (nameOrConnectionString == null) throw new ArgumentNullException("nameOrConnectionString");

            var storage = new MySqlStorage(nameOrConnectionString);
            return configuration.UseStorage(storage);
        }

        public static IGlobalConfiguration<MySqlStorage> UseMySqlStorage(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] string nameOrConnectionString,
            [NotNull] MySqlStorageOptions options)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (nameOrConnectionString == null) throw new ArgumentNullException("nameOrConnectionString");
            if (options == null) throw new ArgumentNullException("options");

            var storage = new MySqlStorage(nameOrConnectionString, options);
            return configuration.UseStorage(storage);
        }
    }
}