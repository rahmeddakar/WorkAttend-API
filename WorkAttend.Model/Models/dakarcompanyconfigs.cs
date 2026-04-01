using System;

namespace WorkAttend.Model.Models
{
    public class dakarcompanyconfigs
    {
        public int companyConfigID { get; set; }
        public int companyId { get; set; }
        public int baseCompanyID { get; set; }
        public string DakarURL { get; set; }
        public string companyCode { get; set; }
        public string siteCode { get; set; }
        public bool isDeleted { get; set; }
        public bool appendCompanyCode { get; set; }
        public int BatchCount { get; set; }
        public DateTime exportStartDate { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}