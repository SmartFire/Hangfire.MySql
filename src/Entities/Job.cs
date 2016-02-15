﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.MySql.Entities.Interfaces;
using LinqToDB;
using LinqToDB.Mapping;

namespace Hangfire.MySql.Entities
{

    // TODO: should this be called MySqlJob - check with SqlStorage

    [Table]
    internal class Job : IHasId
    {
        [PrimaryKey, Identity]
        public int Id { get; set; }
        [Column(DataType = DataType.Text)]
        public string InvocationData { get; set; }
        [Column(DataType = DataType.Text)]
        public string Arguments { get; set; }
        [Column]
        public DateTime CreatedAt { get; set; }
        [Column]
        public DateTime? ExpireAt { get; set; }
        [Column]
        public DateTime? FetchedAt { get; set; }
        [Column]
        public int StateId { get; set; }
        [Column]
        public string StateName { get; set; }
        [Column(DataType = DataType.Text)]
        public string StateReason { get; set; }
        [Column(DataType = DataType.Text)]
        public string StateData { get; set; }
    }

}
