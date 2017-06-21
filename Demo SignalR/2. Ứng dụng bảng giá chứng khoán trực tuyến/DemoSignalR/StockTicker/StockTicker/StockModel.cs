using System;

namespace StockTicker.StockTicker
{
    public class StockModel
    {
        private decimal PriceDefault;
        public string Symbol { get; set; }
        public decimal DayOpen { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal LastChange { get; set; }

        public decimal Price
        {
            get
            {
                return PriceDefault;
            }
            set
            {
                if (PriceDefault == value)
                    return;

                LastChange = value - PriceDefault;
                PriceDefault = value;

                if (DayOpen == 0)
                    DayOpen = PriceDefault;
                if (DayLow == 0 || DayLow > PriceDefault)
                    DayLow = PriceDefault;
                if (PriceDefault > DayHigh)
                    DayHigh = PriceDefault;
            }
        }

        public decimal Change
        {
            get
            {
                return PriceDefault - DayOpen;
            }
        }

        public double PercentChange
        {
            get
            {
                return (double)Math.Round(Change / Price, 4);
            }
        }
    }
}