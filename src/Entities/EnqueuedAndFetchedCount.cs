using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.MySql.Entities
{
    public class EnqueuedAndFetchedCount
    {
        public long? EnqueuedCount { get; set; }
        public long? FetchedCount { get; set; }
    }
}
