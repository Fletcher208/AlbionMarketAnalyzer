using albionMarketAnalyzer.Objects;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace albionMarketAnalyzer
{
    internal class excelExport
    {
        public void ExportCraftableItemsToExcel(List<CraftableItem> items, List<string> baseItemIds)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                
                foreach (var baseItemId in baseItemIds)
                {
                    var itemsOfBase = items.Where(i => i.ItemId.Contains(baseItemId)).ToList();
                    var worksheet = package.Workbook.Worksheets.Add(baseItemId);

                    // Adding headers
                    worksheet.Cells[1, 1].Value = "Item ID";
                    worksheet.Cells[1, 2].Value = "Selling Price";
                    worksheet.Cells[1, 3].Value = "Crafting Cost";
                    worksheet.Cells[1, 4].Value = "Price Delta Category";
                    worksheet.Cells[1, 5].Value = "Selling Location";
                    worksheet.Cells[1, 6].Value = "Last Updated";
                    worksheet.Cells[1, 7].Value = "avg item count";

                    // Find the max number of materials for items of this baseItemId
                    int maxMaterialsCount = itemsOfBase.Max(i => i.RequiredMaterials.Count);

                    // Add Materials & Costs headers
                    for (int i = 0; i < maxMaterialsCount; i++)
                    {
                        worksheet.Cells[1, 8 + i].Value = $"Material & Cost {i + 1}";
                    }

                    // Find the max number of locations for items of this baseItemId
                    int maxLocationsCount = itemsOfBase.Max(i => i.MaterialLocations.Count);

                    // Add Material Locations headers
                    for (int i = 0; i < maxLocationsCount; i++)
                    {
                        worksheet.Cells[1, 8 + maxMaterialsCount + i].Value = $"Material Location {i + 1}";
                    }

                    // Find the max number of last updated timestamps for items of this baseItemId
                    int maxLastUpdatedCount = itemsOfBase.Max(i => i.MateriallastUpdated.Count);

                    // Add Material Last Updated headers
                    for (int i = 0; i < maxLastUpdatedCount; i++)
                    {
                        worksheet.Cells[1, 8 + maxMaterialsCount + maxLocationsCount + i].Value = $"Material Last Updated {i + 1}";
                    }

                    // Adding data for each item
                    int row = 2;
                    foreach (var item in itemsOfBase)
                    {
                        if(item.SellingPrice == 0)
                        {
                            continue;
                        }
                        worksheet.Cells[row, 1].Value = item.ItemId;
                        worksheet.Cells[row, 2].Value = item.SellingPrice;
                        worksheet.Cells[row, 3].Value = item.CraftingCost;
                        worksheet.Cells[row, 4].Value = item.PriceDeltaCategory.ToString();
                        worksheet.Cells[row, 5].Value = item.SellingLocation;
                        worksheet.Cells[row, 6].Value = item.LastUpdated.ToString();
                        worksheet.Cells[row, 7].Value = item.avgItemCount;


                        int col = 8;

                        // Write materials and costs
                        foreach (var material in item.RequiredMaterials)
                        {
                            worksheet.Cells[row, col].Value = $"{material.Key} ({material.Value.Item1}) - {material.Value.Item2}";
                            col++;
                        }

                        // Skip to the right column
                        col = 8 + maxMaterialsCount;

                        // Write material locations
                        foreach (var location in item.MaterialLocations)
                        {
                            worksheet.Cells[row, col].Value = $"{location.Value}";
                            col++;
                        }

                        // Skip to the right column
                        col = 8 + maxMaterialsCount + maxLocationsCount;

                        // Write material last updated timestamps
                        foreach (var lastUpdated in item.MateriallastUpdated)
                        {
                            worksheet.Cells[row, col].Value = $"{lastUpdated.Value}";
                            col++;
                        }

                        row++;
                    }
                }

                bestProfit(package, items, baseItemIds);
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    worksheet.Cells.AutoFitColumns();
                }

                // Save the workbook
                var fileInfo = new FileInfo($"CraftableItems.xlsx");
                package.SaveAs(fileInfo);
            }
        }

        public void bestProfit(ExcelPackage package, List<CraftableItem> items, List<string> baseItemIds)
        {
            // Create a new worksheet for the best profits
            var bestProfitsWorksheet = package.Workbook.Worksheets.Add("BestProfits");

            // Adding headers (same as previous headers)
            bestProfitsWorksheet.Cells[1, 1].Value = "Base Item ID";
            bestProfitsWorksheet.Cells[1, 2].Value = "Item ID";
            bestProfitsWorksheet.Cells[1, 3].Value = "Selling Price";
            bestProfitsWorksheet.Cells[1, 4].Value = "Crafting Cost";
            bestProfitsWorksheet.Cells[1, 5].Value = "Profit";
            bestProfitsWorksheet.Cells[1, 6].Value = "Selling Location";
            bestProfitsWorksheet.Cells[1, 7].Value = "Last Updated";
            bestProfitsWorksheet.Cells[1, 8].Value = "avg Item Count";

            // Adding data for the best profit of each base item
            int row = 2;
            foreach (var baseItemId in baseItemIds)
            {
                var itemsOfBase = items.Where(i => i.ItemId.EndsWith(baseItemId)).ToList();
                var bestProfitItem = itemsOfBase.Where(i => i.avgItemCount > 5).OrderByDescending(i => i.SellingPrice - i.CraftingCost).FirstOrDefault();
                if (bestProfitItem != null)
                {
                bestProfitsWorksheet.Cells[row, 1].Value = baseItemId;
                bestProfitsWorksheet.Cells[row, 2].Value = bestProfitItem.ItemId;
                bestProfitsWorksheet.Cells[row, 3].Value = bestProfitItem.SellingPrice;
                bestProfitsWorksheet.Cells[row, 4].Value = bestProfitItem.CraftingCost;
                bestProfitsWorksheet.Cells[row, 5].Value = bestProfitItem.SellingPrice - bestProfitItem.CraftingCost;  // Profit
                bestProfitsWorksheet.Cells[row, 6].Value = bestProfitItem.SellingLocation;
                bestProfitsWorksheet.Cells[row, 7].Value = bestProfitItem.LastUpdated.ToString();
                bestProfitsWorksheet.Cells[row, 8].Value = bestProfitItem.avgItemCount;
                row++;
                }
                else
                {
                    // Handle the case where no item met the criteria.
                }
            }
            
        }
    }
}
