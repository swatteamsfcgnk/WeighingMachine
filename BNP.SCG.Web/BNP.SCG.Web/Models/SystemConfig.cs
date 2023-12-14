namespace BNP.SCG.Web.Models
{
    public class SystemConfig
    {
        public int id { get; set; }
        public string? register_message { get; set; }
        public string? weight_in_message { get; set; }
        public string? weight_out_message { get; set; }
        public string? over_percentage { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

    }

}
