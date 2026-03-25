using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class PunchIn
    {
        public int punchID { get; set; }
        public int mobileUserID { get; set; }
        public int dakarUserID { get; set; }
        public DateTime timeIn { get; set; }
        public DateTime timeOut { get; set; }
    }
}