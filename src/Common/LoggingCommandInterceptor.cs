using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Logging;
using MySql.Data.MySqlClient;

// ReSharper disable once CheckNamespace
namespace Hangfire.MySql
{
    public class LoggingCommandInterceptor : BaseCommandInterceptor
    {
        protected ILog Logger
        {
            get { return LogProvider.GetCurrentClassLogger(); }
        }

       
        private string FormatSql(string sql)
        {
            return sql.Replace("\r\n", "");
        }


        public override bool ExecuteReader(string sql, CommandBehavior behavior, ref MySqlDataReader returnValue)
        {
            Logger.Trace(DateTime.Now.ToLongTimeString() + " #" + this.ActiveConnection.ServerThread + " " + FormatSql(sql));
            return false;

        }


    }
}
