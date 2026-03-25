using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class ManualPunchesModel
    {
        public int EmployeeId { get; set; }
        public int Id { get; set; }
        public string locationName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public int LocationId { get; set; }
        public string LocationCode { get; set; }
        public string EmployeeEmail { get; set; }
        public double Lattitude { get; set; }
        public double Longitude { get; set; }
        public string Reason { get; set; }

        public DateTime PunchTime { get; set; }
        public int punchType { get; set; }
        public bool isApproved { get; set; }
    }
}