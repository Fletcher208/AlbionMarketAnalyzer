using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static albionMarketAnalyzer.MarketDelta;

namespace albionMarketAnalyzer.Objects
{
    public class CraftableItem
    {
        public CraftableItem()
        {
            RequiredMaterials = new Dictionary<string, (int, double)>();
            MaterialLocations = new Dictionary<string, string>();
            HistoryDataInfoList = new List<HistoryDataInfo>();
            MarketDataInfoList = new List<MarketDataInfo>();
            MateriallastUpdated = new Dictionary<string, DateTime> { };
        }

        public string ItemId { get; set; }
        public Dictionary<string, (int, double)> RequiredMaterials { get; set; }
        public Dictionary<string, string> MaterialLocations { get; set; }
        public Dictionary<string, DateTime> MateriallastUpdated { get; set; }
        public double CraftingCost { get; set; }
        public double SellingPrice { get; set; }
        public string SellingLocation { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsValid { get; set; } = true;

        // To store history data for the item
        public List<HistoryDataInfo> HistoryDataInfoList { get; set; }

        // To store market data for the item
        public List<MarketDataInfo> MarketDataInfoList { get; set; }
        public double PriceDelta { get; set; }
        public PriceDeltaCategory PriceDeltaCategory { get; set; }
        public double avgItemCount { get; set; }
    }
}