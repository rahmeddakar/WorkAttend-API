namespace WorkAttend.API.Gateway.BLL.CommonCode.Helpers
{
    public static class UserAccessContextCacheHelper
    {
        public static string GetDepartmentCacheKey(string userId, string databaseName, int companyId)
        {
            return $"user-access-department:{userId}:{databaseName}:{companyId}";
        }
    }
}