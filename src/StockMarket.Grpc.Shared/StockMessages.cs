namespace StockMarket.Grpc.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class StockData
    {
        [DataMember(Order =1)]
        public string Symbol { get; set; }

        [DataMember(Order = 2)]
        public decimal DayOpen { get; set; }

        [DataMember(Order = 3)]
        public decimal DayLow { get; set; }

        [DataMember(Order = 4)]
        public decimal DayHigh { get; set; }

        [DataMember(Order = 5)]
        public decimal LastChange { get; set; }

        [DataMember(Order = 6)]
        public decimal Price { get; set; }
        
        [DataMember(Order = 7)]
        public DateTime Date { get; set; }
    }

    [DataContract]
    public class StockResult
    {
        [DataMember(Order = 1)]
        public IEnumerable<StockData> Stocks { get; set; }
    }


    [DataContract]
    public class StockRequest
    {
        [DataMember(Order = 1)]
        public string Symbol { get; set; }
    }
}

     