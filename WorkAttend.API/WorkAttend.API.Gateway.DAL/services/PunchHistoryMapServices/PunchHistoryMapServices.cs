using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchHistoryMapServices
{
    public class PunchHistoryMapService : IPunchHistoryMapService
    {
        public async Task<List<punchLocationMarkers>> GetPunchLocationsAsync(
            DateTime startDateParam,
            DateTime endDateParam,
            int employeeId,
            string databaseName)
        {
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsEnd = new TimeSpan(23, 59, 59);

            DateTime startDate = startDateParam.Date + tsStart;
            DateTime endDate = endDateParam.Date + tsEnd;

            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("pl.longitude, pl.latitude, eph.employeepunchhistoryid, l.locationName, eph.punchTime, eph.punchType")
                .From("employeepunchhistory eph")
                .InnerJoin("locations l").On("eph.locationID = l.locationID")
                .InnerJoin("punchlocation pl").On("eph.employeepunchhistoryID = pl.punchhistoryid")
                .Where("eph.employeeid = @0 and eph.punchTime >= @1 and eph.punchTime <= @2", employeeId, startDate, endDate);

            return await Task.FromResult(context.Fetch<punchLocationMarkers>(ppSql).ToList());
        }
    }
}