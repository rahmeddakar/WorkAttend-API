using System.Security.Claims;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.CommonCode.Helpers
{
    public static class CurrentUserContextHelper
    {
        public static CurrentUserContext? Get(ClaimsPrincipal user)
        {
            var userId = user.FindFirst("userId")?.Value
                         ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value;

            var databaseName = user.FindFirst("databaseName")?.Value;
            var companyUrl = user.FindFirst("companyURL")?.Value;

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(databaseName) ||
                string.IsNullOrWhiteSpace(companyUrl))
            {
                return null;
            }

            return new CurrentUserContext
            {
                UserId = userId,
                Email = email ?? string.Empty,
                DatabaseName = databaseName,
                CompanyURL = companyUrl
            };
        }
    }
}