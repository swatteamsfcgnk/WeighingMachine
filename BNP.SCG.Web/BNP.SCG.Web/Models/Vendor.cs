using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BNP.SCG.Web.Models
{
    public class Vendor
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? created_by { get; set; }
        public DateTime created_at { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }

    }
}