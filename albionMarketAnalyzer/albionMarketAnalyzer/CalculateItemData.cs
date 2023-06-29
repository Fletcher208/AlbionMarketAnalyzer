﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class CalculateItemData
    {
        public async Task CalculateCraftingCosts(List<CraftableItem> items, Dictionary<string, double> MaterialPrices, Dictionary<string, string> MaterialLocations)
        {
            foreach (var item in items)
            {
                // Skip calculating crafting costs for materials
                if (item.ItemId.Contains("METALBAR") || item.ItemId.Contains("PLANKS") || item.ItemId.Contains("ARTEFACT") || item.ItemId.Contains("CLOTH") || item.ItemId.Contains("LEATHER")) continue;

                Console.WriteLine($"Calculating costs for {item.ItemId}");

                foreach (var material in item.RequiredMaterials)
                {
                    if (!MaterialPrices.ContainsKey(material.Key) || MaterialPrices[material.Key] == 0)
                    {
                        Console.WriteLine($"No price data for {material.Key}");
                        continue;
                    }

                    double materialCost = MaterialPrices[material.Key] * material.Value;
                    item.CraftingCost += materialCost;
                    item.BestLocations[material.Key] = MaterialLocations[material.Key];
                    item.BestMaterialPrices[material.Key] = MaterialPrices[material.Key];

                    Console.WriteLine($"Added {materialCost} to crafting cost for {material.Value} {material.Key}. Total cost is now {item.CraftingCost}");
                }

            }

            await CalculateSellingPrice(items);
        }

        private async Task CalculateSellingPrice(List<CraftableItem> items)
        {
            var marketDataFetcher = new MarketDataFetcher();

            List<MarketData> marketData = await marketDataFetcher.BuildUri(items);

            // For each item, process the data individually
            foreach (var item in items)
            {
                var itemMarketData = marketData.Where(m => m.item_id == item.ItemId && m.city != "Black Market");
                if (itemMarketData.Where(m => m.sell_price_min > 0).GroupBy(m => m.city).Count() < 4)
                {
                    Console.WriteLine($"Data for {item.ItemId} is not reliable as it is listed in fewer than 4 cities.");
                    continue;
                }

                if (itemMarketData.Any())
                {
                    if (!itemMarketData.Any(m => m.sell_price_min > 0))
                    {
                        Console.WriteLine($"No non-zero selling prices found for {item.ItemId}");
                        continue;
                    }

                    double lowestPrice = itemMarketData.Where(m => m.sell_price_min > 0).Min(m => m.sell_price_min);

                    string bestLocation = itemMarketData.Where(m => m.sell_price_min == lowestPrice).Select(m => m.city).FirstOrDefault();

                    DateTime lastUpdated = itemMarketData.Where(m => m.sell_price_min == lowestPrice).Select(m => m.SellPriceMinDate).FirstOrDefault();

                    item.LastUpdated = lastUpdated;

                    item.SellingPrice = lowestPrice;
                    item.BestSellingLocation = bestLocation;
                    if ((DateTime.Now - lastUpdated).TotalDays > 2)
                        Console.WriteLine($"Data for {item.ItemId} is old!");
                    Console.WriteLine($"Item {item.ItemId} cost {lowestPrice} location {bestLocation}");
                }
                else
                {
                    Console.WriteLine($"No pricing data for {item.ItemId} at any city.");
                }
            }
        }
    }
}