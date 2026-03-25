using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class DakarUserPunch
    {
        public string ClockInTime { get; set; }
        public string ClockOutTime { get; set; }
        public string TotalTime { get; set; }

    }
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }   // if you’re issuing JWT/session tokens
        public bool IsFirstLogin { get; set; }
        public List<string> CompanyFeatures { get; set; }
    }

}

