using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.MySql.src
{
    static public class JobQueueFilters
    {

        internal static IQueryable<Entities.JobQueue> InQueue(this IQueryable<Entities.JobQueue> jobQueues, string queue)
        {
            return jobQueues.Where(jQ => jQ.Queue == queue);
        }

    }
}
