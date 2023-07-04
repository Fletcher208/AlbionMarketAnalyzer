using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer.Objects
{
    public class MarketDataInfo
    {
        public string item_id { get; set; }
        public string City { get; set; }
        public int quality { get; set; }
        public double sell_price_min { get; set; }
    }
}