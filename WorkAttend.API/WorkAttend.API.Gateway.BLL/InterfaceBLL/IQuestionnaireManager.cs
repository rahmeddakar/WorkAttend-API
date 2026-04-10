using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IQuestionnaireManager
    {
        Task<ApiResponse<QuestionaireMod>> GetPageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<List<questionairescale>>> GetScalePageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> UpdateScalesAsync(CurrentUserContext ctx, questionairescale model);
        Task<ApiResponse<List<question>>> GetQuestionnaireAsync(CurrentUserContext ctx, int questionnaireId);
        Task<ApiResponse<bool>> ActivateQuestionnaireAsync(CurrentUserContext ctx, activeQuest model);
        Task<ApiResponse<Questionnaire>> GetAddQuestionnairePageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<Questionnaire>> GetQuestionnaireSettingsPageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> AddQuestionnaireAsync(CurrentUserContext ctx, addQuestionnaire model);
    }
}