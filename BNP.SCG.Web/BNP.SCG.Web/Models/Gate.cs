namespace BNP.SCG.Web.Models
{
    public class Gate
    {
        public int id { get; set; }
        public string? gate_name { get; set; }
        public string? status { get; set; }
        public string? process { get; set; }
        public int? fulfill_id { get; set; }
        public int? customer_id { get; set; }
        public DateTime created_at { get; set; }
    }
}
