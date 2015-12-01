using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;

namespace Hangfire.MySql.src.Entities
{
    [Table]
    internal class Server
    {
        [PrimaryKey]
        public string Id { get; set; }
        [Column(DataType = DataType.Text)]
        public string Data { get; set; }
        [Column]
        public DateTime LastHeartbeat { get; set; }
    }

}
