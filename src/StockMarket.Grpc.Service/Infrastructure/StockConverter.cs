using Google.Protobuf.WellKnownTypes;
using StockMarket.Common;

namespace StockMarket.Grpc.Service.Infrastructure
{
    public static class StockConverter
    {
        private const decimal NanoFactor = 1_000_000_000;

        public static Proto.Decimal FromDecimal(this decimal value)
        {
            var units = decimal.ToInt64(value);
            var nanos = decimal.ToInt32((value - units) * NanoFactor);
            return new Proto.Decimal {Units = units, Nanos = nanos};
        }

        public static decimal ToDecimal(this Proto.Decimal value)
        {
            return value.Units + value.Nanos / NanoFactor;
        }

        public static StockMarket.Grpc.Proto.StockData ToStockData(this Stock stock)
        {
            var stockData = new StockMarket.Grpc.Proto.StockData
            {
                Symbol = stock.Symbol,
                Date = stock.Date.ToTimestamp(),
                DayHigh = stock.High.FromDecimal(),
                DayLow = stock.Low.FromDecimal(),
                DayOpen = stock.Open.FromDecimal()
            };
            return stockData;
        }
    }
}