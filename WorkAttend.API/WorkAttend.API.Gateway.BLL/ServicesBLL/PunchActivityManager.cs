using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.PunchActivityServices;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class PunchActivityManager : IPunchActivityManager
    {
        private readonly IPunchActivityService _punchActivityService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public PunchActivityManager(
            IPunchActivityService punchActivityService,
            IUserAccessContextManager userAccessContextManager)
        {
            _punchActivityService = punchActivityService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<punchActivityListModel>> GetPunchActivitiesAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<punchActivityListModel>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                List<punchattributes> punchAttributes = await _punchActivityService.GetPunchAttributesAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                List<punchAttsVals> punchActivityWithValues = new List<punchAttsVals>();

                foreach (var item in punchAttributes)
                {
                    punchAttsVals punchAttModel = new punchAttsVals
                    {
                        punchAttributeID = item.punchAttributeID,
                        displayName = item.displayName,
                        description = item.description,
                        isCollectDaily = item.isCollectDaily,
                        isMobileAppEnable = item.isMobileAppEnabled,
                        activityTask = await _punchActivityService.GetPunchAttributeValuesAsync(
                            item.punchAttributeID,
                            accessContext.DatabaseName)
                    };

                    punchActivityWithValues.Add(punchAttModel);
                }

                return new ApiResponse<punchActivityListModel>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new punchActivityListModel
                    {
                        punchAttributes = punchActivityWithValues
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchActivityManager.GetPunchActivitiesAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<punchActivityListModel>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> SavePunchActivityAsync(CurrentUserContext ctx, punchActivityModel model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null || string.IsNullOrWhiteSpace(model.activityName))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Kindly fill all fields correctly.",
                        Data = false
                    };
                }

                bool isMobAppEnabled = model.isMobileAppEnable;
                bool isCollectDaily = model.isCollectDaily;

                if (isMobAppEnabled)
                {
                    await _punchActivityService.DisableAllPunchAttributesAsync(
                        accessContext.UserId,
                        accessContext.CompanyId,
                        accessContext.DatabaseName);
                }

                string activityName = RemoveWhitespace(model.activityName);

                punchattributes punchAttribute = await _punchActivityService.SavePunchAttributeAsync(
                    accessContext.DatabaseName,
                    accessContext.UserId,
                    accessContext.CompanyId,
                    activityName,
                    model.activityName,
                    model.activityDescription,
                    isMobAppEnabled,
                    isCollectDaily);

                if (punchAttribute != null && punchAttribute.punchAttributeID > 0 && model.activityTask != null)
                {
                    foreach (var item in model.activityTask)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        await _punchActivityService.SavePunchAttributeValueAsync(
                            accessContext.DatabaseName,
                            accessContext.UserId,
                            punchAttribute.punchAttributeID,
                            item);
                    }
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchActivityManager.SavePunchActivityAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> EditPunchActivityAsync(CurrentUserContext ctx, editPunchModel model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null || model.editActivityID <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = false
                    };
                }

                if (model.editMobileApp)
                {
                    await _punchActivityService.DisableAllPunchAttributesAsync(
                        accessContext.UserId,
                        accessContext.CompanyId,
                        accessContext.DatabaseName);
                }

                bool updated = await _punchActivityService.UpdatePunchAttributeAsync(
                    model.editActivityID,
                    accessContext.UserId,
                    accessContext.DatabaseName,
                    model.editMobileApp,
                    model.editCollectADay);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = updated
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchActivityManager.EditPunchActivityAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        private static string RemoveWhitespace(string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}