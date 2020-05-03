using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockMarket.REST.Server.Models;
using StockMarket.TickerGenerator;

namespace StockMarket.REST.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockMarketController : ControllerBase
    {  
        private readonly ILogger<StockMarketController> _logger;
        private readonly IStockService _stockService;

        public StockMarketController(ILogger<StockMarketController> logger, IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;
        }

        [HttpGet("Get/{symbol}")]
        public async Task<StockModels> Get(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;
            
            var stockHistory = await _stockService.RetrieveStockHistory(symbol.ToUpper(), CancellationToken.None);
            
            var stocks = stockHistory.Take(10).Select(s =>
            {
                return new StockModel
                {
                    Date = s.Date,
                    DayHigh = s.High,
                    DayLow = s.Low,
                    DayOpen = s.Open,
                    DayClose = s.Close,
                    Symbol = s.Symbol

                };
            }).ToArray();
      
            return new StockModels { Stocks = stocks };
        }
        

        [HttpGet("GetPagination/{symbol}/{offset}/{count}")]
        public async Task<StockModels> GetPagination(string symbol, int offset, int count)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;
            
            var stockHistory = await _stockService.RetrieveStockHistory(symbol.ToUpper(), CancellationToken.None);
            
            var stocks = stockHistory.Skip(offset).Take(count).Select(s =>
            {
                return new StockModel
                {
                    Date = s.Date,
                    DayHigh = s.High,
                    DayLow = s.Low,
                    DayOpen = s.Open,
                    DayClose = s.Close,
                    Symbol = s.Symbol

                };
            }).ToArray();

            return new StockModels { Stocks = stocks };
        }        
    }
}
