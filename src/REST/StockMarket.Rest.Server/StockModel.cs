using System;
using System.Collections.Generic;
using System.Text;

namespace StockMarket.Rest.Server
{
    public class StockModel
    {
        public string Symbol { get; set; }
        public decimal DayOpen { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }

    }

    public class StockModels
    {
        public StockModel[] Stocks { get; set; }
    }
}
