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

        private async Task<List<MarketData>> FetchMarketData(string uri)
        {
            string responseBody = await client.GetStringAsync(uri);
            List<MarketData> marketData = JsonConvert.DeserializeObject<List<MarketData>>(responseBody);

            return marketData;
        }

        public Task<List<MarketData>> BuildUri(IEnumerable<string> ids)
        {
            var idsBatchString = string.Join(",", ids);
            return FetchMarketData($"https://east.albion-online-data.com/api/v2/stats/prices/{idsBatchString}.json?locations={locations}&qualities=1");
        }

        public Task<List<MarketData>> BuildUri(IEnumerable<CraftableItem> items)
        {
            var itemBatchString = string.Join(",", items.Select(item => item.ItemId));
            return FetchMarketData($"https://east.albion-online-data.com/api/v2/stats/prices/{itemBatchString}.json?locations={locations}&qualities=1");
        }
    }
}