using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IEmployeeProjHistoryManager
    {
        Task<ApiResponse<Punch>> GetPageDataAsync(CurrentUserContext ctx);
    }
}