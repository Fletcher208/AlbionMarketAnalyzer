using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class GetMaterialData
    {
        public Dictionary<string, DateTime> MaterialLastUpdated { get; set; } = new Dictionary<string, DateTime>();
        public Dictionary<string, double> MaterialPrices { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, string> MaterialLocations { get; set; } = new Dictionary<string, string>();
        private const int dataInCityCount = 4;

        public async Task PopulateMaterialData(List<CraftableItem> items)
        {
            var marketDataFetcher = new MarketDataFetcher();

            List<string> allMaterials = new List<string>();

            foreach (var item in items)
            {
                allMaterials.AddRange(item.RequiredMaterials.Keys);
            }

            allMaterials = allMaterials.Distinct().ToList();  // Remove duplicates

            // Build the URI with all materials
            List<MarketData> marketData = await marketDataFetcher.BuildUri(allMaterials);


            // For each material, process the data individually
            foreach (var material in allMaterials)
            {
                var materialMarketData = marketData.Where(m => m.item_id == material && m.city != "Black Market" && m.sell_price_min > 0).ToList();

                if (materialMarketData.Where(m => m.sell_price_min > 0).GroupBy(m => m.city).Count() < dataInCityCount)
                {
                    Console.WriteLine($"Data for {material} is not reliable as it is listed in fewer than {dataInCityCount} cities.");
                    continue;
                }

                if (materialMarketData.Count() > 0)
                {
                    double lowestPrice = materialMarketData.Min(m => m.sell_price_min);
                    string bestLocation = materialMarketData.Where(m => m.sell_price_min == lowestPrice).Select(m => m.city).FirstOrDefault();

                    DateTime lastUpdated = materialMarketData.Where(m => m.sell_price_min == lowestPrice).Select(m => m.SellPriceMinDate).FirstOrDefault();

                    MaterialLastUpdated[material] = lastUpdated;

                    MaterialPrices[material] = lowestPrice;
                    MaterialLocations[material] = bestLocation;

                    Console.WriteLine($"Price and location data for {material}: {lowestPrice}, {bestLocation}");  // Log the price and location data
                    if ((DateTime.Now - lastUpdated).TotalDays > 2)
                        Console.WriteLine($"Data for {material} is old!");
                }
                else
                {
                    Console.WriteLine($"No non-zero prices found for {material}");  // Log if no non-zero prices were found
                }
            }
        }
    }
}