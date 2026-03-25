using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace WorkAttend.Model.Models
{
    public class companiesDash
    {
        public int companyAdminId { get; set; }
        public int companyId { get; set; }
        public string name { get; set; }
       // public SelectList companiesList { get; set; }
    }
    public class companyListData
    {
        public int companyAdminId { get; set; }
        public int selectedCompanyID { get; set; }
        public string selectedCompanyName { get; set; }
        public List<companiesDash> companyList { get; set; }
    }
}