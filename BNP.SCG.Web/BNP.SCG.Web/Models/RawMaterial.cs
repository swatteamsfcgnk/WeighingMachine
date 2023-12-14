using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BNP.SCG.Web.Models
{
    public class RawMaterial
    {
        public long id { get; set; }
        public string? name { get; set; }
        public int category_id { get; set; }
        public int raw_material_type_id { get; set; }
        public int raw_material_vendor_list_id { get; set; }
        public bool is_active { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

        // public RawMaterialType MyProperty { get; set; }
        public string? category_name { get; set; }
        public string? raw_material_type_name { get; set; }
        public List<int>? vendors_id { get; set; }
        public bool is_rop { get; set; } = false;

    }
}