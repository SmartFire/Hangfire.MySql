using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.MySql.src.Entities.Interfaces;
using LinqToDB.SqlQuery;

namespace Hangfire.MySql.src.Entities.Filters
{
    internal static class IdFilters
    {

        internal static T Row<T>(this IQueryable<T> queryable, int id) where T:IHasId
        {
            return queryable.Single(row => row.Id == id);

        }

    }
}
