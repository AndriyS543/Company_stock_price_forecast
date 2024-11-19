using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock_price_prediction.Models
{
    public class StockPredictionResult
    {
        public string Company { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public double RMSE { get; set; }
        public string ModelFilename { get; set; }
    }
}
