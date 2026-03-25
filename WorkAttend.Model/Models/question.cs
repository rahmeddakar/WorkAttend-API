using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class question
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public bool isDeleted { get; set; }
        public int points { get; set; }
        public int questionID { get; set; }
        public int questionTypeID { get; set; }
        public string text { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
