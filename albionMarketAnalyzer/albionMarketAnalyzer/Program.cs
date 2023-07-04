using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using albionMarketAnalyzer;


public class Program
{

    static async Task Main(string[] args)
    {
        {
            // create an instance of each class
            var itemCreator = new ItemCreator();
            var profitAnalyzer = new ProfitAnalyzer();
            var getMaterialData = new GetMarketData();
            var calculateItemData = new CalculateItemData();
            var calculateItemDelta = new MarketDelta();
            var excelExport = new excelExport();

            List<string> baseItemIds = new List<string> { "MAIN_AXE", "2H_AXE", "2H_HALBERD", "2H_HALBERD_MORGANA", "2H_SCYTHE_HELL", "2H_AXE_AVALON" };
            List<int> tiers = Enumerable.Range(4, 5).ToList();
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

            // Create Items
            var items = itemCreator.CreateCraftableItems(baseItemIds, itemMaterialQuantities, tiers, enchantmentLevels);

            // Get Material data
            await getMaterialData.PopulateMaterialData(items);
            await getMaterialData.PopulateHistoryMaterialData(items);
            calculateItemDelta.CalculatePriceDeltas(items);
            calculateItemData.CalculateCraftingCosts(items);

            // Analyze Profit
            var profitAnalysis = profitAnalyzer.FindBestItemToCraft(items);

            profitAnalyzer.DisplayResults(profitAnalysis);
            excelExport.ExportCraftableItemsToExcel(items, baseItemIds);

        }

    }

}
