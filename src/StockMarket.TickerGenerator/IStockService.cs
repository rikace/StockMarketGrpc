using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StockMarket.Common;

namespace StockMarket.TickerGenerator
{
    public interface IStockService
    {
        Task<IEnumerable<Stock>> RetrieveStockHistory(string ticker, CancellationToken token);
        IAsyncEnumerable<Stock> StockHistoryStream(string ticker, CancellationToken token);
        IAsyncEnumerable<Stock> StockStream(CancellationToken token);
        Task<Stock> SearchStock(string ticker, DateTime date, CancellationToken token);
    }
}