namespace BNP.SCG.Web.Models
{
    public class Silo
    {
        public int id { get; set; }
        public DateTime time { get; set; }
        public long location_id { get; set; }
        public long material_id { get; set; }
        public decimal? value { get; set; } = null;
        public string? unit { get; set; } = null;
        public int rop_id { get; set; }
    }
}
