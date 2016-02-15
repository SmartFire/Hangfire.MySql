using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.MySql.Entities.Interfaces;
using LinqToDB.SqlQuery;

namespace Hangfire.MySql.Entities.Filters
{
    internal static class IdFilters
    {

        internal static T SingleById<T>(this IQueryable<T> queryable, int id) where T:IHasId
        {
            return queryable.Single(row => row.Id == id);
        }

        internal static T SingleById<T>(this IQueryable<T> queryable, string id) where T : IHasId
        {
            return queryable.SingleById(Convert.ToInt32(id));
        }

    }
}
