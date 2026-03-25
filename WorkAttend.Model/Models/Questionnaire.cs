using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkAttend.Model.Models
{
    public class Questionnaire
    {
        public SelectList questionType { get; set; }
        public SelectList questionScaleTypes { get; set; }
    }

    public class activeQuest
    {
        public int questionnaireIDCurr { get; set; }
        public string mobileappFlipQues { get; set; }
    }

    public class questionaireData
    {
        public questionare questionnaireInfo { get; set; }
        public List<question> questions { get; set; }
    }

    public class QuestionaireMod
    {
        public SelectList questionType { get; set; }
       public List<questionaireData> questionnaire { get; set; }
        public SelectList questionScaleTypes { get; set; }


    }
    public class addQuestionnaire
    {
        public string questionnaireTitle { get; set; }
        public int questionType { get; set; }
        public List<string> questionText { get; set; }
        public List<string> points { get; set; }
        public string declaration { get; set; }
        public string topText { get; set; }
        public bool isMobileAppEnable { get; set; }
    }
}