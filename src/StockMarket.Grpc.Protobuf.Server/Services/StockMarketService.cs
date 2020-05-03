using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StockMarket.Grpc.Shared;
using StockMarket.TickerGenerator;

namespace StockMarket.Grpc.Protobuf.Server.Services
{
    public class StockMarketService : IStockMarketService
    {
        private readonly IStockService _stockService;
        
        public StockMarketService(IStockService stockService)
        {
            _stockService = stockService;
        }
        public async ValueTask<StockResult> GetStockAsync(StockRequest request)
        {
            var stockHistory = await _stockService.RetrieveStockHistory(request.Symbol.ToUpper(), CancellationToken.None);

            var stocks = stockHistory.Select(stock => new StockData
            {
                Symbol = stock.Symbol,
                Date = stock.Date,
                DayHigh = stock.High,
                DayLow = stock.Low,
                DayOpen = stock.Open,
                DayClose = stock.Close
            });

            await Task.Delay(2000); // Gotta look busy

            return new StockResult { Stocks = stocks };
        }
    }
}
