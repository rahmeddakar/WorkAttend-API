using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchActivityServices
{
    public interface IPunchActivityService
    {
        Task<List<punchattributes>> GetPunchAttributesAsync(int companyId, string databaseName);
        Task<List<punchattributeValues>> GetPunchAttributeValuesAsync(int punchAttributeId, string databaseName);
        Task<bool> DisableAllPunchAttributesAsync(string userId, int companyId, string databaseName);
        Task<punchattributes> SavePunchAttributeAsync(
            string databaseName,
            string userId,
            int companyId,
            string name,
            string displayName,
            string description,
            bool isMobileAppEnable,
            bool isCollectDaily);

        Task<punchattributeValues> SavePunchAttributeValueAsync(
            string databaseName,
            string userId,
            int attributeId,
            string value);

        Task<bool> UpdatePunchAttributeAsync(
            int punchAttributeId,
            string userId,
            string databaseName,
            bool isMobileAppActive,
            bool isCollectDaily);
    }
}