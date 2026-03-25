using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
namespace WorkAttend.Model.Models
{
    public class departmentsDash
    {
        public int departmentID { get; set; }
        public string departmentName { get; set; }
        public SelectList departmentsList { get; set; }
    }
    public class departmentMod
    {
        public List<department> departments { get; set; }
    }
    public class departmentListData
    {
        public int selectedDeptID { get; set; }
        public string selectedDeptName { get; set; }
        public List<department> departments { get; set; }
    }

    public class dakarIntegrationMod
    {
        public int companyConfigID { get; set; }
        public string DakarURL { get; set; }
        public string CompanyCode { get; set; }
        public string SiteCode { get; set; }
    }
   
}