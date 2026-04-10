using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface ITimeSheetManager
    {
        Task<ApiResponse<TimeSheetPageData>> GetPageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<timeSheetPunchList>> GetTimeSheetAsync(CurrentUserContext ctx, timeSheet model);
        Task<ApiResponse<List<timeSheetEmp>>> GetEmployeeTimeSheetAsync(CurrentUserContext ctx, timeSheetEmployee model);
        Task<ApiResponse<TimeSheetCsvExportResult>> ExportTimeSheetCsvAsync(CurrentUserContext ctx, TimeSheetExportRequest model);
        Task<ApiResponse<projectTimeSheetList>> GetProjectTimeSheetAsync(CurrentUserContext ctx, projectTimeSheet model);
    }
}