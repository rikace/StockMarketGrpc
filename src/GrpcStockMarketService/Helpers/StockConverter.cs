using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockMarket.Grpc.Proto;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Stock = StockHistoryGenerator.StockMarket.Stock;

namespace StockMarket.Grpc.Server.Helpers
{
    public static class StockConverter
    {
        private const decimal NanoFactor = 1_000_000_000;

        public static Proto.Decimal FromDecimal(this decimal value)
        {
            var units = decimal.ToInt64(value);
            var nanos = decimal.ToInt32((value - units) * NanoFactor);
            return new Proto.Decimal { Units = units, Nanos = nanos };
        }

        public static decimal ToDecimal(this Proto.Decimal value)
        { 
            return value.Units + value.Nanos / NanoFactor;
        }


        public static StockMarket.Grpc.Proto.StockData ToStockData(this Stock stock)
        {
            try
            {
                var stockData = new StockMarket.Grpc.Proto.StockData
                {
                    Symbol = stock.Symbol,
                    Date = stock.Date.ToTimestamp(),
                    DayHigh = FromDecimal(stock.DayHigh),
                    DayLow = FromDecimal(stock.DayLow),
                    DayOpen = FromDecimal(stock.DayOpen),
                    LastChange = FromDecimal(stock.LastChange),
                    Price = FromDecimal(stock.Price)
                };
                return stockData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public static Stock ToStock(this StockMarket.Grpc.Proto.StockData stockData)
        {
            var stock = new Stock
            {
                Symbol = stockData.Symbol,
                Date = stockData.Date.ToDateTime(),
                DayHigh = stockData.DayHigh.ToDecimal(),
                DayLow = stockData.DayLow.ToDecimal(),
                DayOpen = stockData.DayOpen.ToDecimal(),
                LastChange = stockData.LastChange.ToDecimal(),
                Price = stockData.Price.ToDecimal(),
            };
            return stock;
        }
    }
}
