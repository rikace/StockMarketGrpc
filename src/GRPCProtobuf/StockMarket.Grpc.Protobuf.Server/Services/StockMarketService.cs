using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StockMarket.Grpc.Shared;
using StockGenerator = StockHistoryGenerator.StockMarket;

namespace StockMarket.Grpc.Protobuf.Server
{
    public class StockMarketService : IStockMarketService
    {
        public async ValueTask<StockResult> GetStockAsync(StockRequest request)
        {
            var stockHistory = await StockGenerator.RetrieveStockHistory(request.Symbol.ToUpper());

            var stocks = stockHistory.Select(stock => new StockData
            {
                Symbol = stock.Symbol,
                Date = stock.Date,
                DayHigh = stock.DayHigh,
                DayLow = stock.DayLow,
                DayOpen = stock.DayOpen,
                LastChange = stock.LastChange,
                Price = stock.Price
            });

            await Task.Delay(2000); // Gotta look busy

            return new StockResult { Stocks = stocks };
        }
    }
}
