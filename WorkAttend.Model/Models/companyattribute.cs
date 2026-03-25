using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class companyattribute
    {
        public int companyAttributeID { get; set; }
        public int companyID { get; set; }
        public int currencyID { get; set; }
        public int countryID { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string city { get; set; }
        public string postcode { get; set; }
        public string phoneNumber { get; set; }
        public string billedTo { get; set; }
        public string billingEmail { get; set; }
        public string expectedAccountType { get; set; }
        public string expectedNumrOfEmp { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
