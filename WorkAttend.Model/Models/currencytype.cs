namespace WorkAttend.Model.Models
{
    public class currencytype
    {
        public int CurrencyTypeID { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public sbyte? IsDelete { get; set; }
        public sbyte? IsActive { get; set; }
    }
}
