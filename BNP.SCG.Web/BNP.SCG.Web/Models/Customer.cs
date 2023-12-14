namespace BNP.SCG.Web.Models
{
    public class Customer
    {
        public long id { get; set; }
        public string? name { get; set; }
        public string? car_license { get; set; }
        public string? phone { get; set; }
        public Guid uniq_data { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

        public string error_message { get; set; } = "";

    }
}
