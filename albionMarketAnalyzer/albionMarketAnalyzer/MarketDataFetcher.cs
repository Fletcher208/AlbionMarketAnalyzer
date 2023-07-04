using albionMarketAnalyzer.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace albionMarketAnalyzer
{
    public class MarketDataFetcher
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string locations = "BlackMarket,Caerleon,Bridgewatch,Thetford,Lymhurst,Martlock,FortSterling";

        private async Task<List<MarketDataInfo>> FetchMarketData(string uri)
        {
            string responseBody = await client.GetStringAsync(uri);
            List<MarketDataInfo> marketData = JsonConvert.DeserializeObject<List<MarketDataInfo>>(responseBody);

            return marketData;
        }

        private async Task<List<HistoryDataInfo>> FetchMarketHistoryData(string uri)
        {
            string responseBody = await client.GetStringAsync(uri);
            List<HistoryDataInfo> marketData = JsonConvert.DeserializeObject<List<HistoryDataInfo>>(responseBody);

            return marketData;
        }

        public Task<List<HistoryDataInfo>> BuildHistoryUri(IEnumerable<string> items, DateTime startDate, DateTime endDate)
        {
            var itemIdsBatchString = string.Join(",", items);
            var startDateString = startDate.ToString("MM-dd-yyyy");
            var endDateString = endDate.ToString("MM-dd-yyyy");

            return FetchMarketHistoryData($"https://east.albion-online-data.com/api/v2/stats/history/{itemIdsBatchString}.json?date={startDateString}&end_date={endDateString}&locations={locations}&time-scale=24");
        }

        public Task<List<HistoryDataInfo>> BuildHistoryUri(IEnumerable<CraftableItem> items, DateTime startDate, DateTime endDate)
        {
            var itemIdsBatchString = string.Join(",", items.Select(item => item.ItemId));
            var startDateString = startDate.ToString("MM-dd-yyyy");
            var endDateString = endDate.ToString("MM-dd-yyyy");

            return FetchMarketHistoryData($"https://east.albion-online-data.com/api/v2/stats/history/{itemIdsBatchString}.json?date={startDateString}&end_date={endDateString}&locations={locations}&time-scale=24");
        }

        public Task<List<MarketDataInfo>> BuildUri(IEnumerable<string> items)
        {
            var itemIdsBatchString = string.Join(",", items);
            return FetchMarketData($"https://east.albion-online-data.com/api/v2/stats/prices/{itemIdsBatchString}.json?locations={locations}&qualities=1");
        }

        public Task<List<MarketDataInfo>> BuildUri(IEnumerable<CraftableItem> items)
        {
            var itemIdsBatchString = string.Join(",", items.Select(item => item.ItemId));
            return FetchMarketData($"https://east.albion-online-data.com/api/v2/stats/prices/{itemIdsBatchString}.json?locations={locations}&qualities=1");
        }
    }
}