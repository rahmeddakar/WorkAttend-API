using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class punchattributeValues
    {
        public int punchAttributeValueID { get; set; }
        public int punchAttributeID { get; set; }
        public string punchAttributeValue { get; set; }
        public bool isDeleted { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
