using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Storage;
using LinqToDB.SqlQuery;
using Newtonsoft.Json;

namespace Hangfire.MySql.src.Entities.Extensions
{
    internal static class JobExtensions
    {

        internal static Hangfire.Common.Job ToCommonJob(this Entities.Job job)
        {
            var invocationData = JsonConvert.DeserializeObject<InvocationData>(job.InvocationData);
            invocationData.Arguments = job.Arguments;
            return invocationData.Deserialize();
        }


        internal static JobData ToJobData(this Entities.Job job)
        {
            var returnValue = new JobData()
            {
                State = job.StateName,
                CreatedAt = job.CreatedAt
            };

            try
            {
                returnValue.Job = job.ToCommonJob();
            }
            catch (JobLoadException ex)
            {
                returnValue.LoadException = ex;
            }

            return returnValue;
        }

        internal static bool IsInState(this Entities.Job job, string stateName)
        {
            return job.StateName == stateName;
        }


        internal static StateData ToStateData(this Entities.Job job)
        {
            return new StateData()
            {
                Name = job.StateName,
                Reason = job.StateReason,
                Data = JsonConvert.DeserializeObject<IDictionary<string,string>>(job.StateData)
            };
        }

        internal static DateTime? GetNullableDateTimeStateDataValue(this Entities.Job job, string stateDataKey)
        {
            var sd = job.ToStateData();
            return JobHelper.DeserializeNullableDateTime(sd.Data[stateDataKey]);
        }

    }

}