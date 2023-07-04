using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer.Objects
{
    public class HistoryDataPoint
    {
        public int item_count { get; set; }
        public double avg_price { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class HistoryDataInfo
    {
        public  string location { get; set; }
        public  string item_id { get; set; }
        public int quality { get; set; }
        public  List<HistoryDataPoint> data { get; set; }
    }

}
