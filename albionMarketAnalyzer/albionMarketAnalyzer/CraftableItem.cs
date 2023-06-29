using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class CraftableItem
    {
        public CraftableItem()
        {
            RequiredMaterials = new Dictionary<string, int>();
            BestLocations = new Dictionary<string, string>();
            BestMaterialPrices = new Dictionary<string, double>();
        }

        public string ItemId { get; set; }
        public Dictionary<string, int> RequiredMaterials { get; set; }
        public double CraftingCost { get; set; }
        public Dictionary<string, string> BestLocations { get; set; }
        public Dictionary<string, double> BestMaterialPrices { get; set; }
        public double SellingPrice { get; set; }
        public string BestSellingLocation { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsValid { get; set; } = true;
    }
}