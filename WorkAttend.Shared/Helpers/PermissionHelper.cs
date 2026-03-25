using System.Collections.Generic;
using System.Text.Json;

namespace WorkAttend.Shared.Helpers
{
    public static class PermissionHelper
    {
        public static List<string> GetAllowedActions(string policy, string controllerName)
        {
            var actionsAllowed = new List<string>();

            if (string.IsNullOrWhiteSpace(policy) || string.IsNullOrWhiteSpace(controllerName))
                return actionsAllowed;

            using var document = JsonDocument.Parse(policy);

            if (!document.RootElement.TryGetProperty(controllerName, out var controllerActions))
                return actionsAllowed;

            foreach (var item in controllerActions.EnumerateArray())
            {
                actionsAllowed.Add(item.GetString()?.ToLower() ?? string.Empty);
            }

            return actionsAllowed;
        }
    }
}
