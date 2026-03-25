using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class projectClientList
    {
       public int projectID { get; set; }
       public string projectName { get; set; }
       public string projectCode { get; set; }
       public string description { get; set; }
        public int clientID { get; set; }
        public string clientName { get; set; }
        public string clientCode { get; set; }
        public string createdBy { get; set; }
       public DateTime createdOn { get; set; }
       public string updatedBy { get; set; }
       public DateTime updatedOn { get; set; }
        public int locationID { get; set; }
        public bool isForAdd { get; set; }

        // For Activties
        public int activityID { get; set; }
        public string name { get; set; }
        public string color { get; set; }
    }
    public class projectMod
    {
        public List<projectClientList> projectClientList {get; set;}
        public List<clients> clientList { get; set; }
        public List<Employees> employeesData { get; set; }
        public List<Project> Projects { get; set; }
        public List<Location> Locations { get; set; }
    }
    public class ActivityMod
    {
        public List<Activity> Activities { get; set; }
    }
}