using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BNP.SCG.Web.Models
{
    public class PrintFinishModel
    {

        public string id { get; set; }
        public string car_license { get; set; }
        public int is_print { get; set; }
        public DateTime? create_date { get; set; }
        public decimal diff { get; set; }
        public string document_no { get; set; }        
        public string supplier_name { get; set; }
        public string raw_material_name { get; set; }
        public string location_name { get; set; }
        public decimal weight_register { get; set; }
        public decimal weight_in { get; set; }
        public decimal weight_out { get; set; }
        public decimal percentage_diff { get; set; }
        public DateTime? weight_in_at { get; set; }
        public DateTime? weight_out_at { get; set; }
        public decimal Ticket_Diff { get; set; }
    }
}