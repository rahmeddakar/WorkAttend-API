using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchHistoryMapServices
{
    public interface IPunchHistoryMapService
    {
        Task<List<punchLocationMarkers>> GetPunchLocationsAsync(
            DateTime startDateParam,
            DateTime endDateParam,
            int employeeId,
            string databaseName);
    }
}