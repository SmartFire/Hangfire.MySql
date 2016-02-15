using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;

namespace Hangfire.MySql.Entities
{
    [Table]
    internal class JobParameter
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column]
        public int JobId { get; set; }
        [Column]
        public string Name { get; set; }
        [Column(DataType = DataType.Text)]
        public string Value { get; set; }
    }

}
