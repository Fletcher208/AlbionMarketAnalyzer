using albionMarketAnalyzer.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class GetMarketData
    {
        private const int dataInCityCount = 3;

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
            List<MarketDataInfo> marketData = await marketDataFetcher.BuildUri(allMaterials);
            
            
            // For each material, process the data individually
            foreach (var material in allMaterials)
            {
                var materialMarketData = marketData.Where(m => m.item_id == material && m.City != "Black Market" && m.sell_price_min > 0).ToList();
                

                if (materialMarketData.Where(m => m.sell_price_min > 0).GroupBy(m => m.City).Count() < dataInCityCount)
                {
                    Console.WriteLine($"Data for {material} is not reliable as it is listed in fewer than {dataInCityCount} cities.");
                    continue;
                }

                if (materialMarketData.Count() > 0)
                {
                    double lowestPrice = materialMarketData.Min(m => m.sell_price_min);
                    string Location = materialMarketData.First(m => m.sell_price_min == lowestPrice).City;
                    DateTime lastUpdated = DateTime.Now;

                    // Update CraftableItem with the data from materialMarketData
                    UpdateCraftableItemsWithMaterialData(items, material, lowestPrice, Location, lastUpdated, materialMarketData);
                }
                else
                {
                    Console.WriteLine($"No non-zero prices found for {material}");  // Log if no non-zero prices were found
                }
            }

            List<MarketDataInfo> itemMarketData = await marketDataFetcher.BuildUri(items);

            foreach (var item in items)
            {
                var itemMarketDataInfo = itemMarketData.Where(m => m.item_id == item.ItemId && m.City != "Black Market" && m.sell_price_min > 0).ToList();

                if (itemMarketDataInfo.Count() > 0)
                {
                    double lowestPrice = itemMarketDataInfo.Min(m => m.sell_price_min);
                    string Location = itemMarketDataInfo.First(m => m.sell_price_min == lowestPrice).City;
                    DateTime lastUpdated = DateTime.Now;

                    item.SellingPrice = lowestPrice;
                    item.SellingLocation = Location;
                    item.LastUpdated = lastUpdated;
                    item.MarketDataInfoList.AddRange(itemMarketDataInfo);
                }
                else
                {
                    Console.WriteLine($"No non-zero prices found for item {item.ItemId}");
                }
            }
        }

        public async Task PopulateHistoryMaterialData(List<CraftableItem> items)
        {
            var marketDataFetcher = new MarketDataFetcher();

            List<string> allMaterials = new List<string>();

            foreach (var item in items)
            {
                allMaterials.AddRange(item.RequiredMaterials.Keys);
            }

            allMaterials = allMaterials.Distinct().ToList();  // Remove duplicates

            // Build the URI with all materials
            List<HistoryDataInfo> marketHistoryData = await marketDataFetcher.BuildHistoryUri(allMaterials, DateTime.Now.AddDays(-10), DateTime.Now);

            // For each material, process the data individually
            foreach (var material in allMaterials)
            {
                var materialHistoryData = marketHistoryData.Where(h => h.item_id == material && h.location != "Black Market").ToList();

                if (materialHistoryData.SelectMany(h => h.data.Select(d => d.avg_price)).Distinct().Count() < 2)
                {
                    Console.WriteLine($"Data for {material} is not reliable as it has fewer than 2 unique prices.");
                    continue;
                }

                if (materialHistoryData.Any())
                {
                    var mostRecentDataPoint = materialHistoryData.SelectMany(h => h.data.Select(d => new { h.location, h.item_id, d.item_count, d.avg_price, d.timestamp })).OrderByDescending(d => d.timestamp).First();

                    double mostRecentPrice = mostRecentDataPoint.avg_price;
                    string Location = mostRecentDataPoint.location;
                    DateTime lastUpdated = mostRecentDataPoint.timestamp;

                    // Update CraftableItem with the data from materialHistoryData
                    UpdateCraftableItemsWithHistoryData(items, material, mostRecentPrice, Location, lastUpdated, materialHistoryData);
                }
                else
                {
                    Console.WriteLine($"No data found for {material}");
                }
            }

            // Now, process historical data for the items themselves
            List<HistoryDataInfo> itemHistoryData = await marketDataFetcher.BuildHistoryUri(items, DateTime.Now.AddDays(-14), DateTime.Now);

            foreach (var item in items)
            {
                var itemHistoryDataInfo = itemHistoryData.Where(h => h.item_id == item.ItemId).ToList();

                if (itemHistoryDataInfo.Any())
                {
                    // Flatten data into one list, include location, item_id, item_count, avg_price and timestamp
                    var flatData = itemHistoryDataInfo.SelectMany(h => h.data.Select(d => new { h.location, h.item_id, d.item_count, d.avg_price, d.timestamp })).ToList();

                    // Determine the most recent timestamp for each city.
                    var mostRecentTimestampPerCity = flatData.GroupBy(d => d.location).Select(g => new { Location = g.Key, Timestamp = g.Max(d => d.timestamp) }).ToList();

                    // Find the data point with the lowest price for the most recent timestamp in each city.
                    var bestDataPoints = new List<dynamic>();
                    foreach (var cityTimestamp in mostRecentTimestampPerCity)
                    {
                        var dataForCityAndTimestamp = flatData.Where(d => d.location == cityTimestamp.Location && d.timestamp == cityTimestamp.Timestamp);
                        var bestDataForCityAndTimestamp = dataForCityAndTimestamp.OrderBy(d => d.avg_price).First();
                        bestDataPoints.Add(bestDataForCityAndTimestamp);
                    }

                    // Now select the best overall data point from the best data points per city.
                    var bestOverallDataPoint = bestDataPoints.OrderBy(d => d.avg_price).First();

                    double bestPrice = bestOverallDataPoint.avg_price;
                    string bestLocation = bestOverallDataPoint.location;
                    DateTime mostRecentTimestamp = bestOverallDataPoint.timestamp;

                    if(item.SellingPrice < bestPrice) 
                    {
                        item.SellingPrice = bestPrice;
                        item.SellingLocation = bestLocation;
                        item.LastUpdated = mostRecentTimestamp;
                    }
                    item.avgItemCount = flatData.Average(d => d.item_count);
                    item.HistoryDataInfoList.AddRange(itemHistoryDataInfo);
                }
                else
                {
                    Console.WriteLine($"No historical data found for item {item.ItemId}");
                }
            }
        }

        private void UpdateCraftableItemsWithMaterialData(List<CraftableItem> items, string material, double lowestPrice, string Location, DateTime lastUpdated, List<MarketDataInfo> materialMarketData)
        {
            foreach (var item in items)
            {
                if (item.RequiredMaterials.ContainsKey(material))
                {
                    // update price of the material
                    var oldTuple = item.RequiredMaterials[material];
                    item.RequiredMaterials[material] = (oldTuple.Item1, lowestPrice);
                    if (!item.MateriallastUpdated.ContainsKey(material))
                    {
                        item.MateriallastUpdated.Add(material, lastUpdated);
                    }
                    else if (item.MateriallastUpdated[material] < lastUpdated)
                    {
                        item.MateriallastUpdated[material] = lastUpdated;
                    }
                    // update best MaterialLocations
                    item.MaterialLocations[material] = Location;

                    // add material market data to the list
                    item.MarketDataInfoList.AddRange(materialMarketData);
                }
            }
        }

        private void UpdateCraftableItemsWithHistoryData(List<CraftableItem> items, string material, double mostRecentPrice, string Location, DateTime lastUpdated, List<HistoryDataInfo> materialHistoryData)
        {
            foreach (var item in items)
            {
                if (item.RequiredMaterials.ContainsKey(material))
                {
                    // update price of the material
                    var oldTuple = item.RequiredMaterials[material];
                    item.RequiredMaterials[material] = (oldTuple.Item1, mostRecentPrice);
                    if (!item.MateriallastUpdated.ContainsKey(material))
                    {
                        item.MateriallastUpdated.Add(material, lastUpdated);
                    }
                    else if (item.MateriallastUpdated[material] < lastUpdated)
                    {
                        item.MateriallastUpdated[material] = lastUpdated;
                    }
                    // update best MaterialLocations
                    item.MaterialLocations[material] = Location;

                    // add material history data to the list
                    item.HistoryDataInfoList.AddRange(materialHistoryData);
                }
            }
        }
    }
}