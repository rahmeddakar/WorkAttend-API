namespace WorkAttend.Model.Models
{
    public class DataTableResponse<T>
    {
        public int draw { get; set; }
        public long recordsTotal { get; set; }
        public long recordsFiltered { get; set; }
        public List<T> data { get; set; } = new List<T>();
    }
}