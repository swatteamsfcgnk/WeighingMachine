namespace BNP.SCG.Web.Models
{
    public class User
    {
        public long id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? email { get; set; }
        public string? role { get; set; }
        public bool is_active { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }


        public string? error { get; set; }

    }
}