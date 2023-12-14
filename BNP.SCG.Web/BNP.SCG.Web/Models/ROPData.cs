namespace BNP.SCG.Web.Models
{
    public class ROPData
    {
        public long id { get; set; }
        public long material_id { get; set; }
        public string material_name { get; set; }
        public string material_type_name { get; set; }

        public long location_id { get; set; }
        public string location_name { get; set; }

        public decimal rop { get; set; } = 0;
        public decimal usge_qty { get; set; } = 0;
        public decimal max_qty { get; set; } = 0;
        public int shift { get; set; } = 0;
        public string remark { get; set; }

        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

        public bool is_rop { get; set; }
        public bool is_active { get; set; }

        public int category_id { get; set; }
        public string? category_name { get; set; }

    }

    public class ROPAddOnData
    {
        public long id { get; set; }
        public long rop_id { get; set; }
        public long location_id { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }

    }

    public class RMRemainData : ROPData
    {
        public decimal remain_qty { get; set; }
        public decimal cal { get; set; }
    }
}
