using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BNP.SCG.Web.Models
{
    public class RawMaterialType
    {
        public long id { get; set; }
        public string? name { get; set; }
        public bool is_rop { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}