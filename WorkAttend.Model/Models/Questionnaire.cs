using System;
using System.Collections.Generic;

namespace WorkAttend.Model.Models
{
    public class Questionnaire
    {
        public List<questiontype> questionType { get; set; } = new();
        public List<questionnairescaletype> questionScaleTypes { get; set; } = new();
    }

    public class activeQuest
    {
        public int questionnaireIDCurr { get; set; }
        public string mobileappFlipQues { get; set; }
    }

    public class questionaireData
    {
        public questionaire questionnaireInfo { get; set; }
        public List<question> questions { get; set; } = new();
    }

    public class QuestionaireMod
    {
        public List<questiontype> questionType { get; set; } = new();
        public List<questionaireData> questionnaire { get; set; } = new();
        public List<questionnairescaletype> questionScaleTypes { get; set; } = new();
    }

    public class addQuestionnaire
    {
        public string questionnaireTitle { get; set; }
        public int questionType { get; set; }
        public List<string> questionText { get; set; } = new();
        public List<string> points { get; set; } = new();
        public string declaration { get; set; }
        public string topText { get; set; }
        public bool isMobileAppEnable { get; set; }
    }

    public class questionaire
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string declaration { get; set; }
        public string headerText { get; set; }
        public bool isDeleted { get; set; }
        public bool isMobileAppActive { get; set; }
        public string name { get; set; }
        public int questionaireID { get; set; }
        public int totalPoints { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class question
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public bool isDeleted { get; set; }
        public int points { get; set; }
        public int questionID { get; set; }
        public int questionTypeID { get; set; }
        public string text { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class questiontype
    {
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public bool isDeleted { get; set; }
        public int questionTypeID { get; set; }
        public string text { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class questionnairescaletype
    {
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public int questionnaireScaleTypeID { get; set; }
        public int scale { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class questionairescale
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public int endRange { get; set; }
        public string name { get; set; }
        public int questionaireScaleID { get; set; }
        public int startRange { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }

    public class questionairequestion
    {
        public string createdby { get; set; }
        public DateTime createdon { get; set; }
        public int questionId { get; set; }
        public int questionireId { get; set; }
        public int qusetionairequestionid { get; set; }
        public string updatedby { get; set; }
        public DateTime updatedon { get; set; }
    }
}