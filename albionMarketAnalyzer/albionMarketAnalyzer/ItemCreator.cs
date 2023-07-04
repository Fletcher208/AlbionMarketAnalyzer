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
    public class ItemCreator
    {
        public List<CraftableItem> CreateCraftableItems(List<string> baseItemIds, Dictionary<string, Dictionary<string, int>> itemMaterialQuantities, List<int> tiers, List<int> enchantmentLevels)
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

                                // Add the quantity and set the price as zero for now
                                item.RequiredMaterials[materialId] = (kvp.Value, 0);
                            }
                        }
                        // Add to the list of items
                        items.Add(item);
                    }
                }
            }

            return items;
        }
    }
}