using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class MarketData
    {
        public string city { get; set; }
        public string item_id { get; set; }
        public double sell_price_min { get; set; }
        public int quality { get; set; }
        public double buy_price_min { get; set; }

        [JsonProperty("sell_price_min_date")]
        public string SellPriceMinDateStr { get; set; }

        [JsonIgnore]
        public DateTime SellPriceMinDate
        {
            get
            {
                DateTime.TryParse(SellPriceMinDateStr, out DateTime parsedDate);
                return parsedDate;
            }
        }
    }
}