using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;

namespace StockTicker.StockTicker
{
    [HubName("stockTicker")]
    public class StockTickerHub : Hub
    {
        private StockTicker instance;
        private readonly StockTicker _stockTicker;

        public StockTickerHub() : this(StockTicker.Instance)
        {
        }

        public StockTickerHub(StockTicker stockTicker)
        {
            this._stockTicker = stockTicker;
        }

        public IEnumerable<StockModel> GetAllStocks()
        {
            return _stockTicker.GetAllStocks();
        }

        public string GetMarketState()
        {
            return _stockTicker.MarketState.ToString();
        }

        public void OpenMarket()
        {
            _stockTicker.OpenMarket();
        }

        public void CloseMarket()
        {
            _stockTicker.CloseMarket();
        }

        public void Reset()
        {
            _stockTicker.Reset();
        }

        public void Hello()
        {
            Clients.All.hello();
        }
    }
}