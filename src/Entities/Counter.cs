using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace Hangfire.MySql.Entities
{

    /**
     
     * CREATE TABLE [HangFire].[Counter](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[Key] [nvarchar](100) NOT NULL,
			[Value] [tinyint] NOT NULL,
			[ExpireAt] [datetime] NULL,

     * 
     */

    [Table]
    public class Counter
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }

        [Column]
        public string Key { get; set; }

        [Column]
        public int Value { get; set; }

        [Column]
        public DateTime? ExpireAt { get; set; }


        public static int IncrementValue = 1;
        public static int DecrementValue = -1;


    }


}
