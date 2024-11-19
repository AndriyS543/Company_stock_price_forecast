using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock_price_prediction.Models
{
    public class CompanyData
    {
        public string Company { get; set; }
        public string Ticker { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }
}
