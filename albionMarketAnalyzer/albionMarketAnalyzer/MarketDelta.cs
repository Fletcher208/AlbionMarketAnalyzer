using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using albionMarketAnalyzer.Objects;

namespace albionMarketAnalyzer
{
    public class MarketDelta
    {
        
        public enum PriceDeltaCategory
        {
            VeryPositive,
            Positive,
            Neutral,
            Negative,
            VeryNegative
        }
        
        public void CalculatePriceDeltas(List<CraftableItem> items)
        {

            const int days = 7;
            foreach (var item in items)
            {
                // Sort HistoryDataInfoList by timestamp
                var sortedHistoryData = item.HistoryDataInfoList.SelectMany(h => h.data.Select(d => new { h.location, h.item_id, d.item_count, d.avg_price, d.timestamp })).OrderByDescending(d => d.timestamp).ToList();

                // Check if there's enough data to compute PriceDelta
                if (sortedHistoryData.Count < days)
                {
                    Console.WriteLine($"Not enough historical data to compute PriceDelta for {item.ItemId}");
                    continue;
                }

                // Compute the average of the last 'days' prices
                double averageHistoricalPrice = sortedHistoryData.Take(days).Average(h => h.avg_price);

                // Compute and update PriceDelta for the item
                item.PriceDelta = item.SellingPrice - averageHistoricalPrice;

                // Categorize the PriceDelta
                if (item.PriceDelta > 500000)
                {
                    item.PriceDeltaCategory = PriceDeltaCategory.VeryPositive;
                }
                else if (item.PriceDelta > 0)
                {
                    item.PriceDeltaCategory = PriceDeltaCategory.Positive;
                }
                else if (item.PriceDelta == 0)
                {
                    item.PriceDeltaCategory = PriceDeltaCategory.Neutral;
                }
                else if (item.PriceDelta > -500000)
                {
                    item.PriceDeltaCategory = PriceDeltaCategory.Negative;
                }
                else
                {
                    item.PriceDeltaCategory = PriceDeltaCategory.VeryNegative;
                }
            }
        }
    }
}
