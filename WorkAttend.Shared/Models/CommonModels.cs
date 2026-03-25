using System;
using System.Collections.Generic;
using System.Text;

namespace WorkAttend.Shared.Models
{
    class CommonModels
    {
    }

    public class EntityLangValuesModel
    {
        public int EntityID { get; set; }
        public int RowID { get; set; }
        public int LangID { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PageLabelModel
    {
        public string Key { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LabelID { get; set; }
        public int ValueID { get; set; }
        public int AppID { get; set; }
        public int LangID { get; set; }
    }
  
}
