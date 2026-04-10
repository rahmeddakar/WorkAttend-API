using System.Collections.Generic;

namespace WorkAttend.Model.Models
{
    public class TimeSheetPageData
    {
        public List<Employees> employees { get; set; } = new List<Employees>();
        public List<Location> locations { get; set; } = new List<Location>();
    }
}