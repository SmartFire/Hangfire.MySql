using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire.MySql.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Extensions;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;

namespace Hangfire.MySql.src
{
    internal class Setup : ShortConnectingDatabaseActor
    {
        public Setup(string connectionString) : base(connectionString)
        {
          
        }

        public void EnsureDatabase()
        {
            var types =
              Assembly.GetAssembly(typeof(Setup))
                  .GetTypes()
                  .Where(t => t.GetCustomAttributes(typeof(TableAttribute), true).Length > 0)
                  .ToList();

            UsingDatabase(db =>
            {
                var sp = db.DataProvider.GetSchemaProvider();
                var dbSchema = sp.GetSchema(db);
                foreach (var type in types)
                {
                    var attr = type.GetAttributes<TableAttribute>().First();
                    var name = string.IsNullOrEmpty(attr.Name) ? type.Name : attr.Name;
                    if (!dbSchema.Tables.Any(t => t.TableName.ToLowerInvariant() == name.ToLowerInvariant()))
                    {
                        MethodInfo method = typeof(DataExtensions).GetMethod("CreateTable", new[] { typeof(DataConnection), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(DefaulNullable) });
                        MethodInfo generic = method.MakeGenericMethod(type);
                        generic.Invoke(this, new object[] { db, null, null, null, null, null, null });
                    }
                }
            });


        }


    }
}
