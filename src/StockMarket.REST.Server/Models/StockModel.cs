using System;

namespace StockMarket.REST.Server.Models
{
    public class StockModel
    {
        public string Symbol { get; set; }
        public decimal DayOpen { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayClose { get; set; }
        public DateTime Date { get; set; }

    }

    public class StockModels
    {
        public StockModel[] Stocks { get; set; }
    }
}
