using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.QuestionnaireServices
{
    public class QuestionnaireService : IQuestionnaireService
    {
        public async Task<List<questionaire>> GetQuestionnaireAsync(int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("questionaire")
                .Where("isDeleted != 1 and companyID = @0", companyId)
                .OrderBy("questionaireid desc");

            return await Task.FromResult(context.Fetch<questionaire>(ppSql).ToList());
        }

        public async Task<List<question>> GetQuestionnaireQuestAsync(int questionnaireId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("q.*")
                .From("questionairequestions qq")
                .InnerJoin("questions q").On("qq.questionId = q.questionId")
                .InnerJoin("questionaire ques").On("qq.questionireId = ques.questionaireID")
                .Where("q.isDeleted != 1 and ques.isDeleted != 1 and qq.questionireID = @0", questionnaireId);

            return await Task.FromResult(context.Fetch<question>(ppSql).ToList());
        }

        public async Task<List<questiontype>> GetQuestionTypesAsync(string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("questiontypes")
                .Where("isDeleted != 1");

            return await Task.FromResult(context.Fetch<questiontype>(ppSql).ToList());
        }

        public async Task<List<questionnairescaletype>> GetQuestionScaleTypesAsync(string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("questionnairescaletype");

            return await Task.FromResult(context.Fetch<questionnairescaletype>(ppSql).ToList());
        }

        public async Task<List<questionairescale>> GetQuestScalesAsync(int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("qs.*")
                .From("questionairescales qs")
                .Where("qs.companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<questionairescale>(ppSql).ToList());
        }

        public async Task<bool> UpdateScalesAsync(int questScaleId, int startRange, int endRange, string userId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("questionairescales")
                .Where("questionairescaleID = @0", questScaleId);

            var questScale = context.Fetch<questionairescale>(ppSql).FirstOrDefault();

            if (questScale == null || questScale.questionaireScaleID <= 0)
                return await Task.FromResult(false);

            questScale.startRange = startRange;
            questScale.endRange = endRange;
            questScale.updatedOn = DateTime.Now;
            questScale.updatedBy = userId;

            context.Update(questScale);

            return await Task.FromResult(true);
        }

        public async Task<bool> ActivateQuestionnaireAsync(int questId, string userId, int companyId, bool isActive, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var allSql = Sql.Builder
                .Select("*")
                .From("questionaire")
                .Where("companyID = @0", companyId);

            var allQuestionnaires = context.Fetch<questionaire>(allSql).ToList();

            foreach (var item in allQuestionnaires)
            {
                item.isMobileAppActive = false;
                item.updatedOn = DateTime.Now;
                item.updatedBy = userId;
                context.Update(item);
            }

            var targetSql = Sql.Builder
                .Select("*")
                .From("questionaire")
                .Where("questionaireID = @0 and companyID = @1", questId, companyId);

            var target = context.Fetch<questionaire>(targetSql).FirstOrDefault();

            if (target == null)
                return await Task.FromResult(false);

            target.isMobileAppActive = isActive;
            target.updatedOn = DateTime.Now;
            target.updatedBy = userId;

            context.Update(target);

            return await Task.FromResult(true);
        }

        public async Task<question> AddQuestionAsync(string text, int points, int companyId, string userId, string databaseName)
        {
            DateTime now = DateTime.Now;

            question newQuestion = new question
            {
                text = text,
                companyID = companyId,
                questionTypeID = 1,
                points = points,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            object id = context.Insert(newQuestion);
            newQuestion.questionID = Convert.ToInt32(id);

            return await Task.FromResult(newQuestion);
        }

        public async Task<questionairequestion> AddQuestionareQuestAsync(int questionId, int questionnaireId, string userId, string databaseName)
        {
            DateTime now = DateTime.Now;

            questionairequestion newQues = new questionairequestion
            {
                questionId = questionId,
                questionireId = questionnaireId,
                createdon = now,
                createdby = userId,
                updatedon = now,
                updatedby = userId
            };

            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            object id = context.Insert(newQues);
            newQues.qusetionairequestionid = Convert.ToInt32(id);

            return await Task.FromResult(newQues);
        }

        public async Task<questionaire> AddQuestionnaireAsync(string questTitle, string questDeclaration, int companyId, string userId, int totalPoints, string topText, string databaseName, bool mobileAppEnable)
        {
            DateTime now = DateTime.Now;
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            if (mobileAppEnable)
            {
                var existingSql = Sql.Builder
                    .Select("*")
                    .From("questionaire")
                    .Where("companyID = @0", companyId);

                var existing = context.Fetch<questionaire>(existingSql).ToList();

                foreach (var item in existing)
                {
                    item.isMobileAppActive = false;
                    item.updatedOn = now;
                    item.updatedBy = userId;
                    context.Update(item);
                }
            }

            questionaire newQuestionnaire = new questionaire
            {
                companyID = companyId,
                name = questTitle,
                declaration = questDeclaration,
                headerText = topText,
                totalPoints = totalPoints,
                isMobileAppActive = mobileAppEnable,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            object id = context.Insert(newQuestionnaire);
            newQuestionnaire.questionaireID = Convert.ToInt32(id);

            return await Task.FromResult(newQuestionnaire);
        }
    }
}