using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkAttend.Model.Models
{
    public class JobsModel
    {
        public List<Jobs> Jobs { get; set; }
    }
    public class Jobs
    {
        public int Id { get; set; }
        public string Job { get; set; }
    }
    public class JobViewModel
    {
        public int JobID { get; set; }
        public string name { get; set; }
        public bool isForAdd { get; set; }
    }
}