namespace BNP.SCG.Web.Models
{
    public class FulfillData
    {
        public long id { get; set; }
        public DateTime date_in { get; set; }
        public string date_in_str { get; set; }
        public string date_out_str { get; set; }
        public DateTime? date_out { get; set; }
        public string car_license { get; set; }
        public long supplier_id { get; set; }
        public string supplier_name { get; set; } = string.Empty;
        public long raw_material_id { get; set; }
        public string raw_material_name { get; set; } = string.Empty;
        public string document_no { get; set; } = string.Empty;
        public decimal weight_in { get; set; }
        public decimal weight_out { get; set; }
        public decimal weight_diff { get; set; }
        public decimal weight_in_2 { get; set; } = 0;
        public decimal weight_out_2 { get; set; } = 0;
        public decimal weight_diff_2 { get; set; } = 0;
        public long location_id { get; set; }
        public string location_name { get; set; } = string.Empty;
        public long location_id_2 { get; set; }
        public string location_name_2 { get; set; } = string.Empty;
        public decimal weight_register { get; set; }
        public decimal percentage_diff { get; set; }
        public decimal percentage_diff_2 { get; set; }
        public decimal weight_silo_before { get; set; }
        public decimal weight_silo_after { get; set; }
        public decimal weight_silo_before_2 { get; set; }
        public decimal weight_silo_after_2 { get; set; }

        public Guid uniq_data { get; set; }

        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

        public string deleted_by { get; set; }
        public DateTime? deleted_at { get; set; }
        public bool is_deleted { get; set; } = false;

        public string doc_status { get; set; }

        public string error_message { get; set; } = "";

        public string weight_in_by { get; set; }
        public DateTime? weight_in_at { get; set; }
        public string weight_out_by { get; set; }
        public DateTime? weight_out_at { get; set; }


        public string? show_doc_status_thai { get; set; }
        public bool second_load { get; set; } = false;
        public decimal category_fix_percentage_diff { get; set; }

    }
}
