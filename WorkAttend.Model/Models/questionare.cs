using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class questionare
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string declaration { get; set; }
        public string headerText { get; set; }
        public bool isDeleted { get; set; }
        public bool isMobileAppActive { get; set; }
        public string name { get; set; }
        public int questionaireID { get; set; }
        public int totalPoints { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
