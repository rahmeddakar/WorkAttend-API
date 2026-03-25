namespace WorkAttend.Model.Models
{
    public class RegisterCompanyPageData
    {
        public List<country> countries { get; set; } = new List<country>();
        public List<currencytype> currencies { get; set; } = new List<currencytype>();
        public List<industry> industries { get; set; } = new List<industry>();
    }
}