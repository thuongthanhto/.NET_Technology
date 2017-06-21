using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace StockTicker.StockTicker
{
    public class StockTicker
    {
        private readonly static Lazy<StockTicker> _instance = new Lazy<StockTicker>(() => new StockTicker(GlobalHost.ConnectionManager.GetHubContext<StockTickerHub>().Clients));
        private readonly ConcurrentDictionary<string, StockModel> _stockModel = new ConcurrentDictionary<string, StockModel>();

        // status Market
        private readonly object _marketStateLock = new object();

        private readonly object _updateStockPricesLock = new object();
        private readonly Random _updateOrNotRandom = new Random();
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        private readonly double _rangePercent = 0.002;

        private Timer _timer;
        private volatile bool _updatingStockPrices;
        private volatile MarketStates _marketState;

        public StockTicker(Microsoft.AspNet.SignalR.Hubs.IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            LoadDefaultStocks();
        }

        public static StockTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public MarketStates MarketState
        {
            get { return _marketState; }
            set { _marketState = value; }
        }

        public IEnumerable<StockModel> GetAllStocks()
        {
            return _stockModel.Values;
        }

        public void OpenMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketStates.Open)
                {
                    _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);
                    MarketState = MarketStates.Open;
                    BroadcastMarketStateChange(MarketStates.Open);
                }
            }
        }

        public void CloseMarket()
        {
            lock (_marketStateLock)
            {
                if (MarketState == MarketStates.Open)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }

                    MarketState = MarketStates.Closed;

                    BroadcastMarketStateChange(MarketStates.Closed);
                }
            }
        }

        public void Reset()
        {
            lock (_marketStateLock)
            {
                if (MarketState != MarketStates.Closed)
                {
                    throw new InvalidOperationException("Phái đóng trước khi Reset");
                }

                LoadDefaultStocks();
                BroadcastMarketReset();
            }
        }

        private void LoadDefaultStocks()
        {
            _stockModel.Clear();

            var stocks = new List<StockModel>
            {
                new StockModel { Symbol = "A", Price = 100.00m },
                new StockModel { Symbol = "B", Price = 120.00m },
                new StockModel { Symbol = "C", Price = 150.00m },
                new StockModel { Symbol = "D", Price = 80.00m },
            };

            stocks.ForEach(stock => _stockModel.TryAdd(stock.Symbol, stock));
        }

        private void UpdateStockPrices(object state)
        {
            lock (_updateStockPricesLock)
            {
                if (!_updatingStockPrices)
                {
                    _updatingStockPrices = true;

                    foreach (var stock in _stockModel.Values)
                    {
                        if (TryUpdateStockPrice(stock))
                        {
                            BroadcastStockPrice(stock);
                        }
                    }

                    _updatingStockPrices = false;
                }
            }
        }

        private bool TryUpdateStockPrice(StockModel stock)
        {
            var r = _updateOrNotRandom.NextDouble();
            if (r > 0.1)
            {
                return false;
            }

            var random = new Random((int)Math.Floor(stock.Price));
            var percentChange = random.NextDouble() * _rangePercent;
            var pos = random.NextDouble() > 0.51;
            var change = Math.Round(stock.Price * (decimal)percentChange, 2);
            change = pos ? change : -change;

            stock.Price += change;
            return true;
        }

        private void BroadcastMarketReset()
        {
            Clients.All.marketReset();
        }

        private void BroadcastStockPrice(StockModel stock)
        {
            Clients.All.updateStockPrice(stock);
        }

        private void BroadcastMarketStateChange(MarketStates marketState)
        {
            switch (marketState)
            {
                case MarketStates.Open:
                    Clients.All.marketOpened();
                    break;

                case MarketStates.Closed:
                    Clients.All.marketClosed();
                    break;

                default:
                    break;
            }
        }
    }

    public enum MarketStates
    {
        Closed,
        Open
    }
}