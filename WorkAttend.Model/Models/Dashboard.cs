using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class DashboardStats
    {
        public int totalEmployees { get; set; }
        public int totalIn { get; set; }
        public int totalOut { get; set; }
        public int onSite { get; set; }
        public int lowRisk { get; set; }
        public int highRisk { get; set; }
        public int mediumRisk { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<resultsQuest> resultList {get; set;}
        public string dateFilter { get; set; }
    }
    public class resultsQuest
    {
        public string email { get; set; }
        public int questionaireResultsID { get; set; }
        public int questionaireScaleID { get; set; }
        public int range { get; set; }
        public string rangeText { get; set; }
        public string name { get; set; }
    }
}