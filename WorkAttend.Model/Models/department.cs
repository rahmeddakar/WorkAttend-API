using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class department
    {
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string departmentCode { get; set; }
        public int departmentID { get; set; }
        public string departmentName { get; set; }
        public bool isDeleted { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
