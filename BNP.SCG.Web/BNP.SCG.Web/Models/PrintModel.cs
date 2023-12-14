using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BNP.SCG.Web.Models
{
    public class PrintFinishModel
    {

        public string  id  { get; set; }
        public string  car_license { get; set; }
        public int  is_print { get; set; }
        public DateTime  create_date { get; set; }
        public decimal  diff { get; set; }

    }
}