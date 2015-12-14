using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Hangfire.MySql.src.Entities.Filters;
using Hangfire.MySql.src.Entities.Interfaces;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using MySql.Data.MySqlClient;

namespace Hangfire.MySql.src
{
    public abstract class DatabaseActor
    {

        /// <summary>
        /// 
        /// </summary>
        public static IDataProvider DataProvider = new MySqlDataProvider();


        protected readonly DateTime? NullDateTime = null;

        


        protected void UsingTable<TEntity>(Action<ITable<TEntity>> action) where TEntity : class
        {
            UsingDatabase(db => action(db.GetTable<TEntity>()));
        }

        protected TResult UsingTable<TEntity, TResult>(Func<ITable<TEntity>, TResult> action) where TEntity : class
        {
            return UsingDatabase(db => action(db.GetTable<TEntity>()));
        }


        protected void UsingDatabase(Action<DataConnection> action)
        {
            UsingDatabase(dc => { action(dc); return true; });
        }

        protected TResult UsingDatabase<TResult>(Func<DataConnection, TResult> func)
        {
            return Invoke(func);
        }

        protected TResult Get<TResult>(int id) where TResult: class, IHasId
        {
            return UsingTable<TResult, TResult>(table => table.SingleById(id));
        }

        protected TResult Get<TResult>(string id) where TResult : class, IHasId
        {
            return UsingTable<TResult, TResult>(table => table.SingleById(id));
        }

        protected abstract TResult Invoke<TResult>(Func<DataConnection, TResult> func);

        
    }
}
