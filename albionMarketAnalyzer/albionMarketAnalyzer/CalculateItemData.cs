using albionMarketAnalyzer.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class CalculateItemData
    {
        private const int dataInCityCount = 2;

        public void CalculateCraftingCosts(List<CraftableItem> items)
        {
            foreach (var item in items)
            {
                double totalCraftingCost = 0;
                foreach (var material in item.RequiredMaterials)
                {
                    totalCraftingCost += material.Value.Item1 * material.Value.Item2; // material.Value.Item1 is the quantity and material.Value.Item2 is the price
                }
                item.CraftingCost = totalCraftingCost;
            }
        }
    }
}