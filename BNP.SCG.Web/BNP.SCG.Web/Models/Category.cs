namespace BNP.SCG.Web.Models
{
    public class Category
    {
        public int id { get; set; }
        public string? name { get; set; }
        public decimal fix_percentage_diff { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

    }
}