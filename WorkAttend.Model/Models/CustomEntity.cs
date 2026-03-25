using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class CustomEntity
    {
        public class User
        {
            public int UserID { get; set; }

            public int DakarUserID { get; set; }

            public string Manufacturer { get; set; }

            public string DeviceID { get; set; }
        }
    }
}