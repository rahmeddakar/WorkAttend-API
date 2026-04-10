using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IUserAccessContextManager
    {
        Task<UserAccessContext?> GetAsync(CurrentUserContext ctx, bool forceRefresh = false);
        void Remove(string databaseName, string userId);
    }
}