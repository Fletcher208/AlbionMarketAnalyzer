using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

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

public class Program
{
    static readonly HttpClient client = new HttpClient();
    static Dictionary<string, double> MaterialPrices = new Dictionary<string, double>();
    static Dictionary<string, string> MaterialLocations = new Dictionary<string, string>();
    static string locations = "BlackMarket,Caerleon,Bridgewatch,Thetford,Lymhurst,Martlock,FortSterling";
    static Dictionary<string, DateTime> MaterialLastUpdated = new Dictionary<string, DateTime>();



    static async Task Main(string[] args)
    {
        
        try
        {
            string tier = Console.ReadLine();
            // Define your base item IDs, base material IDs, tiers, and enchantment levels
            List<string> baseItemIds = new List<string> { "MAIN_AXE", "2H_AXE", "2H_HALBERD", "2H_HALBERD_MORGANA", "2H_SCYTHE_HELL", "2H_AXE_AVALON" };
            

            List<int> tiers = Enumerable.Range(4, Convert.ToInt32(tier)-3).ToList();
            List<int> enchantmentLevels = Enumerable.Range(0, 5).ToList();

            Dictionary<string, Dictionary<string, int>> itemMaterialQuantities = new Dictionary<string, Dictionary<string, int>>
            {
                { "MAIN_AXE", new Dictionary<string, int> { { "METALBAR", 16 },{ "PLANKS", 8 } } },
                { "2H_AXE", new Dictionary<string, int> { { "METALBAR", 20 }, { "PLANKS", 12 } } },
                { "2H_HALBERD", new Dictionary<string, int> { { "METALBAR", 12 }, { "PLANKS", 20 } } },
                { "2H_HALBERD_MORGANA", new Dictionary<string, int> { { "METALBAR", 12 }, { "PLANKS", 20 }, { "ARTEFACT_2H_HALBERD_MORGANA", 1 } } },
                { "2H_SCYTHE_HELL", new Dictionary<string, int> { { "METALBAR", 12 }, { "PLANKS", 20 }, { "ARTEFACT_2H_SCYTHE_HELL", 1 } } },
                { "2H_AXE_AVALON", new Dictionary<string, int> { { "METALBAR", 12 }, { "PLANKS", 20 }, { "ARTEFACT_2H_AXE_AVALON", 1 } } }
            };

            // Create normal and enchanted items
            List<CraftableItem> items = CreateCraftableItems(baseItemIds, itemMaterialQuantities, tiers, enchantmentLevels);

            await PopulateMaterialData(items);
            await CalculateCraftingCosts(items);
            //await DetermineBestSellingLocations(items);
            CraftableItem bestItem = FindBestItemToCraft(items);

            DisplayResults(bestItem);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

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
    }

    public static async Task CalculateCraftingCosts(List<CraftableItem> items)
    {
        foreach (var item in items)
        {
            // Skip calculating crafting costs for materials
            if (item.ItemId.Contains("METALBAR")|| item.ItemId.Contains("PLANKS")|| item.ItemId.Contains("ARTEFACT")) continue;

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


    public static CraftableItem FindBestItemToCraft(List<CraftableItem> items)
    {
        CraftableItem bestItem = null;

        // Find the first item with a positive profit margin
        foreach (var item in items)
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

        foreach (var item in items)
        {
            if ((item.SellingPrice - item.CraftingCost) > (bestItem.SellingPrice - bestItem.CraftingCost))
            {
                bestItem = item;
            }
        }

        return bestItem;
    }

    public static async Task CalculateSellingPrice(List<CraftableItem> items)
    {
        // Build the URI with all items
        string uri = BuildSellingPriceUri(items);
        string responseBody = await client.GetStringAsync(uri);

        List<MarketData> marketData = JsonConvert.DeserializeObject<List<MarketData>>(responseBody);

        // For each item, process the data individually
        foreach (var item in items)
        {
            var itemMarketData = marketData.Where(m => m.item_id == item.ItemId && m.city != "Black Market");

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



    // Display result in a separate method
    public static void DisplayResults(CraftableItem bestItem)
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
            Console.WriteLine($"\t{material.Key}: {material.Value} units from {bestItem.BestLocations[material.Key]} at a price of {bestItem.BestMaterialPrices[material.Key]} each.");
        }
        Console.WriteLine($"The best location to sell {bestItem.ItemId} is {bestItem.BestSellingLocation} for a price of {bestItem.SellingPrice}.");
    }

    public static async Task PopulateMaterialData(List<CraftableItem> items)
    {
        List<string> allMaterials = new List<string>();

        foreach (var item in items)
        {
            allMaterials.AddRange(item.RequiredMaterials.Keys);
        }

        allMaterials = allMaterials.Distinct().ToList();  // Remove duplicates

        // Build the URI with all materials
        string uri = BuildMaterialDataUri(allMaterials);
        string responseBody = await client.GetStringAsync(uri);

        List<MarketData> marketData = JsonConvert.DeserializeObject<List<MarketData>>(responseBody);

        // For each material, process the data individually
        foreach (var material in allMaterials)
        {
            var materialMarketData = marketData.Where(m => m.item_id == material && m.city != "Black Market" && m.sell_price_min > 0).ToList();

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

    public static List<CraftableItem> CreateCraftableItems(List<string> baseItemIds, Dictionary<string, Dictionary<string, int>> itemMaterialQuantities, List<int> tiers, List<int> enchantmentLevels)
    {
        List<CraftableItem> items = new List<CraftableItem>();

        foreach (int tier in tiers)
        {
            foreach (int enchantmentLevel in enchantmentLevels)
            {
                // Create normal and enchanted items
                foreach (string baseItemId in baseItemIds)
                {
                    CraftableItem item = new CraftableItem();

                    // Set the item ID based on the tier and enchantment level
                    item.ItemId = "T" + tier + "_" + baseItemId;

                    if (enchantmentLevel > 0)
                    {
                        item.ItemId += "@";
                        item.ItemId += enchantmentLevel;
                    }

                    if (itemMaterialQuantities.TryGetValue(baseItemId, out Dictionary<string, int> materialQuantities))
                    {
                        // The required materials also depend on the tier and enchantment level
                        foreach (KeyValuePair<string, int> kvp in materialQuantities)
                        {
                            string materialId = "T" + tier + "_" + kvp.Key;
                            if (enchantmentLevel > 0 && !materialId.Contains("ARTEFACT"))
                            {
                                materialId += "_LEVEL";
                                materialId += enchantmentLevel;
                                materialId += "@";
                                materialId += enchantmentLevel;
                            }

                            item.RequiredMaterials[materialId] = kvp.Value;
                        }
                    }
                    // Add to the list of items
                    items.Add(item);
                }
            }
        }

        return items;
    }

    public static string BuildMaterialDataUri(IEnumerable<string> materialIds)
    {
        var materialBatchString = string.Join(",", materialIds);
        return $"https://east.albion-online-data.com/api/v2/stats/prices/{materialBatchString}.json?locations={locations}&qualities=1";
    }

    public static string BuildSellingPriceUri(IEnumerable<CraftableItem> items)
    {
        var itemBatchString = string.Join(",", items.Select(item => item.ItemId));
        return $"https://east.albion-online-data.com/api/v2/stats/prices/{itemBatchString}.json?locations={locations}&qualities=1";
    }
}
