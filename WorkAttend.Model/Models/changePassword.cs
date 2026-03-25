using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class changePassword
    {
        public string userName { get; set; }
        public string newPassword { get; set; }
        public string oldPassword { get; set; }
    }
}