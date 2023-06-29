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

            // Filter valid items only
            var validItems = items.Where(item => item.IsValid).ToList();

            // Find the first item with a positive profit margin
            foreach (var item in validItems)
            {
                Console.WriteLine($"{item.SellingPrice}, {item.CraftingCost} ");
                if (item.SellingPrice - item.CraftingCost > 0)
                {
                    bestItem = item;
                    break;
                }
            }

            // If no items have a positive profit margin, return null
            if (bestItem == null)
                return null;

            // Iterate again through valid items to find the best one
            foreach (var item in validItems)
            {
                if ((item.SellingPrice - item.CraftingCost) > (bestItem.SellingPrice - bestItem.CraftingCost))
                {
                    bestItem = item;
                }
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
                Console.WriteLine("Material Details:");
                foreach (var material in bestItem.RequiredMaterials)
                {
                if (bestItem.BestLocations.ContainsKey(material.Key) && bestItem.BestMaterialPrices.ContainsKey(material.Key))
                {
                    Console.WriteLine($"\t{material.Key}: {material.Value} units from {bestItem.BestLocations[material.Key]} at a price of {bestItem.BestMaterialPrices[material.Key]} each.");
                }
                else
                {
                    Console.WriteLine($"No data available for {material.Key}");
                }
            }
                Console.WriteLine($"The best location to sell {bestItem.ItemId} is {bestItem.BestSellingLocation} for a price of {bestItem.SellingPrice}.");
            }
        }
}