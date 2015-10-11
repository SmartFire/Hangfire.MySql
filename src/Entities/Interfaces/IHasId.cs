using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.MySql.src.Entities.Interfaces
{
    public interface IHasId
    {
        int Id { get; }
    }
}
