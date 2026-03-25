using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class punchAttributeModel
    {
        public List<punchAttributeList> punchattributeValues { get; set; }
        public string attributeDisplayName { get; set; }
        public string attributeDescription { get; set; }
        public bool isCollectDaily { get; set; }
    }

    public class punchAttributeList
    {
        public int punchAttributeValueID { get; set; }

       public int punchAttributeID { get; set; }

        public string punchAttributeValue { get; set; }

        public bool isDeleted { get; set; }

        public bool isChecked { get; set; } = false;
    }
}