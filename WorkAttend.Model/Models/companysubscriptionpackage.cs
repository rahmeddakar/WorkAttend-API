using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class companysubscriptionpackage
    {
        public int companySubscriptionPackageID { get; set; }
        public int SubscriptionPackageID { get; set; }
        public int companyID { get; set; }
        public DateTime packageStartDate { get; set; }
        public DateTime packageEndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
