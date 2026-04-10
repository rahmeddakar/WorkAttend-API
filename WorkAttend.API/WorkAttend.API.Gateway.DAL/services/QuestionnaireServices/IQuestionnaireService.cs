using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.QuestionnaireServices
{
    public interface IQuestionnaireService
    {
        Task<List<questionaire>> GetQuestionnaireAsync(int companyId, string databaseName);
        Task<List<question>> GetQuestionnaireQuestAsync(int questionnaireId, string databaseName);
        Task<List<questiontype>> GetQuestionTypesAsync(string databaseName);
        Task<List<questionnairescaletype>> GetQuestionScaleTypesAsync(string databaseName);
        Task<List<questionairescale>> GetQuestScalesAsync(int companyId, string databaseName);

        Task<bool> UpdateScalesAsync(int questScaleId, int startRange, int endRange, string userId, string databaseName);
        Task<bool> ActivateQuestionnaireAsync(int questId, string userId, int companyId, bool isActive, string databaseName);

        Task<question> AddQuestionAsync(string text, int points, int companyId, string userId, string databaseName);
        Task<questionairequestion> AddQuestionareQuestAsync(int questionId, int questionnaireId, string userId, string databaseName);
        Task<questionaire> AddQuestionnaireAsync(string questTitle, string questDeclaration, int companyId, string userId, int totalPoints, string topText, string databaseName, bool mobileAppEnable);

    }
}