using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class questionnaireModel
    {
        public List<question> questions { get; set; }
        public string title { get; set; }
        public string declaration { get; set; }
        public string headerText { get; set; }
    }
    public class saveEmployeeQuest
    {
        public List<questionEmployee> questionAnswers { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int punchType { get; set; }
        public string employeeEmail { get; set; }
        public int employeeID { get; set; }
        public int companyID { get; set; }
        public int attributeValueID { get; set; }
        public string attributeValue { get; set; }
        public string picture { get; set; }
        public int activityId { get; set; }
        public int projectId { get; set; }
        public string jobIds { get; set; }
        //public int questionaireID { get; set; }
    }
    public class questionEmployee
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
        /// <summary>
        /// 
        /// </summary>
       // public int questionID { get; set; }
        //public string text { get; set; }
        public List<questionTypes> options { get; set; }
        // public int companyID { get; set; }
        //public int questionTypeID { get; set; }
        //public int points { get; set; }
        ////public string updatedBy { get; set; }
        ////public DateTime updatedOn { get; set; }
        ////public string createdBy { get; set; }
        ////public DateTime createdOn { get; set; }
        //public bool isDeleted { get; set; }

    }

    public class questionTypes
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool isChecked { get; set; }
    }

    public class employeeQuestionAnswers
    {
        public int employeeID { get; set; }
        public List<question> questions { get; set; }
    }
}