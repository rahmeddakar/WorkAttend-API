using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.QuestionnaireServices;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class QuestionnaireManager : IQuestionnaireManager
    {
        private const string CurrentControllerName = "questionnaire";

        private readonly IQuestionnaireService _questionnaireService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public QuestionnaireManager(
            IQuestionnaireService questionnaireService,
            IUserAccessContextManager userAccessContextManager)
        {
            _questionnaireService = questionnaireService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<QuestionaireMod>> GetPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<QuestionaireMod>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                List<questionaire> questionnaires = await _questionnaireService.GetQuestionnaireAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                List<questionaireData> questData = new List<questionaireData>();

                foreach (var item in questionnaires)
                {
                    questionaireData modelQuest = new questionaireData
                    {
                        questionnaireInfo = item,
                        questions = await _questionnaireService.GetQuestionnaireQuestAsync(item.questionaireID, accessContext.DatabaseName)
                    };

                    questData.Add(modelQuest);
                }

                List<questiontype> types = await _questionnaireService.GetQuestionTypesAsync(accessContext.DatabaseName);
                List<questionnairescaletype> scaleTypes = await _questionnaireService.GetQuestionScaleTypesAsync(accessContext.DatabaseName);

                QuestionaireMod model = new QuestionaireMod
                {
                    questionnaire = questData,
                    questionType = types ?? new List<questiontype>(),
                    questionScaleTypes = scaleTypes ?? new List<questionnairescaletype>()
                };

                return new ApiResponse<QuestionaireMod>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.GetPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<QuestionaireMod>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<List<questionairescale>>> GetScalePageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<List<questionairescale>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var scales = await _questionnaireService.GetQuestScalesAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                return new ApiResponse<List<questionairescale>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = scales ?? new List<questionairescale>()
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.GetScalePageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<questionairescale>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateScalesAsync(CurrentUserContext ctx, questionairescale model)
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

                if (model == null || model.questionaireScaleID <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                bool isUpdated = await _questionnaireService.UpdateScalesAsync(
                    model.questionaireScaleID,
                    model.startRange,
                    model.endRange,
                    accessContext.UserId.ToString(),
                    accessContext.DatabaseName);

                return new ApiResponse<bool>
                {
                    Success = isUpdated,
                    Message = isUpdated ? "Processed successfully." : "Something went wrong.",
                    Data = isUpdated
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.UpdateScalesAsync",
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

        public async Task<ApiResponse<List<question>>> GetQuestionnaireAsync(CurrentUserContext ctx, int questionnaireId)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<List<question>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var questionsData = await _questionnaireService.GetQuestionnaireQuestAsync(
                    questionnaireId,
                    accessContext.DatabaseName);

                return new ApiResponse<List<question>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = questionsData ?? new List<question>()
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.GetQuestionnaireAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<question>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> ActivateQuestionnaireAsync(CurrentUserContext ctx, activeQuest model)
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

                if (!HasPermission(accessContext.Policy, CurrentControllerName, "update"))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.questionnaireIDCurr <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                bool isActive = false;
                if (!string.IsNullOrWhiteSpace(model.mobileappFlipQues) &&
                    model.mobileappFlipQues.ToLower() == "on")
                {
                    isActive = true;
                }

                bool isActivated = await _questionnaireService.ActivateQuestionnaireAsync(
                    model.questionnaireIDCurr,
                    accessContext.UserId.ToString(),
                    accessContext.CompanyId,
                    isActive,
                    accessContext.DatabaseName);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = isActivated
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.ActivateQuestionnaireAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<Questionnaire>> GetAddQuestionnairePageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<Questionnaire>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                List<questiontype> types = await _questionnaireService.GetQuestionTypesAsync(accessContext.DatabaseName);
                List<questionnairescaletype> scaleTypes = await _questionnaireService.GetQuestionScaleTypesAsync(accessContext.DatabaseName);

                Questionnaire model = new Questionnaire
                {
                    questionType = types ?? new List<questiontype>(),
                    questionScaleTypes = scaleTypes ?? new List<questionnairescaletype>()
                };

                return new ApiResponse<Questionnaire>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.GetAddQuestionnairePageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<Questionnaire>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<Questionnaire>> GetQuestionnaireSettingsPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<Questionnaire>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                List<questiontype> types = await _questionnaireService.GetQuestionTypesAsync(accessContext.DatabaseName);

                Questionnaire model = new Questionnaire
                {
                    questionType = types ?? new List<questiontype>()
                };

                return new ApiResponse<Questionnaire>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.GetQuestionnaireSettingsPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<Questionnaire>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> AddQuestionnaireAsync(CurrentUserContext ctx, addQuestionnaire model)
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

                if (!HasPermission(accessContext.Policy, CurrentControllerName, "create"))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.questionText == null || model.points == null || model.questionText.Count == 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                List<int> createdQuestions = new List<int>();
                int totalPoints = 0;

                for (int i = 0; i < model.questionText.Count; i++)
                {
                    question quest = await _questionnaireService.AddQuestionAsync(
                        model.questionText[i],
                        int.Parse(model.points[i]),
                        accessContext.CompanyId,
                        accessContext.UserId.ToString(),
                        accessContext.DatabaseName);

                    createdQuestions.Add(quest.questionID);
                    totalPoints += int.Parse(model.points[i]);
                }

                questionaire questionnaire = await _questionnaireService.AddQuestionnaireAsync(
                    model.questionnaireTitle,
                    model.declaration,
                    accessContext.CompanyId,
                    accessContext.UserId.ToString(),
                    totalPoints,
                    model.topText,
                    accessContext.DatabaseName,
                    model.isMobileAppEnable);

                foreach (var item in createdQuestions)
                {
                    await _questionnaireService.AddQuestionareQuestAsync(
                        item,
                        questionnaire.questionaireID,
                        accessContext.UserId.ToString(),
                        accessContext.DatabaseName);
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "QuestionnaireManager.AddQuestionnaireAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        private static bool HasPermission(string policy, string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(policy) || string.IsNullOrWhiteSpace(controllerName))
                return false;

            try
            {
                JObject policyJson = JObject.Parse(policy);
                List<string> actionsAllowed = new List<string>();

                if (policyJson.ContainsKey(controllerName))
                {
                    foreach (var item in policyJson[controllerName])
                    {
                        actionsAllowed.Add(item.ToString().ToLower());
                    }
                }

                return actionsAllowed.Contains(actionName.ToLower());
            }
            catch
            {
                return false;
            }
        }
    }
}