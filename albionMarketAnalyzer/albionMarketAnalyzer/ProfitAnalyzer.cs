using albionMarketAnalyzer.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace albionMarketAnalyzer
{
    public class ProfitAnalyzer
    {
        public CraftableItem FindBestItemToCraft(List<CraftableItem> items)
        {
            CraftableItem bestItem = null;
            double bestProfitWeighted = 0;

            // Filter valid items only
            var validItems = items.Where(item => item.IsValid).ToList();

            // Iterate through valid items to find the best one
            foreach (var item in validItems)
            {
                // Get the total sales volume for the item
                int totalSalesVolume = item.HistoryDataInfoList.Sum(h => h.data.Sum(d => d.item_count));

                // Calculate profit margin
                double profitMargin = item.SellingPrice - item.CraftingCost;

                // Calculate profit margin weighted by sales volume
                double profitWeighted = profitMargin * totalSalesVolume;

                // Compare with the best profit so far
                if (profitWeighted > bestProfitWeighted)
                {
                    bestProfitWeighted = profitWeighted;
                    bestItem = item;
                }
                Console.WriteLine($"{item.ItemId}, {item.SellingPrice}, {item.CraftingCost}, {item.PriceDelta}, {item.SellingLocation}");
            }

            return bestItem;
        }

        public void DisplayResults(CraftableItem bestItem)
        {
            if (bestItem == null)
            {
                Console.WriteLine("No craftable items found with a non-zero crafting cost.");
                return;
            }

            Console.WriteLine($"The best item to craft is {bestItem.ItemId} with a crafting cost of {bestItem.CraftingCost}.");
            Console.WriteLine($"Market price delta: {bestItem.PriceDelta}");
            Console.WriteLine("Material Details:");
            foreach (var material in bestItem.RequiredMaterials)
            {
                if (bestItem.MaterialLocations.ContainsKey(material.Key))
                {
                    int materialQuantity = material.Value.Item1;
                    double materialPrice = material.Value.Item2;
                    string materialLocation = bestItem.MaterialLocations[material.Key];

                    Console.WriteLine($"\t{material.Key}: {materialQuantity} units from {materialLocation} at a price of {materialPrice} each.");
                }
                else
                {
                    Console.WriteLine($"No data available for {material.Key}");
                }
            }
            Console.WriteLine($"The best location to sell {bestItem.ItemId} is {bestItem.SellingLocation} for a price of {bestItem.SellingPrice}.");
        }
    }
}