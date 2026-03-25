using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class subscriptionfeature
    {
        public int SubscriptionFeatureID { get; set; }
        public string FeatureName { get; set; }
        public string DisplayName { get; set; }
        public string FeatureDescription { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSystem { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDefault { get; set; }
    }
}
