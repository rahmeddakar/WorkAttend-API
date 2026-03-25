using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class punchActivityModel
    {
        public string activityName { get; set; }
        public string activityDescription { get; set; }
        public List<string> activityTask { get; set; }
        public bool isMobileAppEnable { get; set; }
        public bool isCollectDaily { get; set; }
    }

    public class editPunchModel
    {
        public int editActivityID { get; set; }
        public bool editMobileApp { get; set; }
        public bool editCollectADay { get; set; }
    }
    public class punchActivityListModel
    {
        public List<punchAttsVals> punchAttributes { get; set; }
    }

    public class punchAttsVals
    {
        public int punchAttributeID { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public bool isMobileAppEnable { get; set; }
        public bool isCollectDaily { get; set; }
        public List<punchattributeValues> activityTask { get; set; }
    }
}