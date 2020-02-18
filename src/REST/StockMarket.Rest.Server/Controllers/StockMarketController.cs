using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockGenerator = StockHistoryGenerator.StockMarket;

namespace StockMarket.Rest.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockMarketController : ControllerBase
    {  
        private readonly ILogger<StockMarketController> _logger;

        public StockMarketController(ILogger<StockMarketController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<StockModels> Get()
        {
            var stockHistory = await StockGenerator.RetrieveStockHistory("AAPL");
            var stocks = stockHistory.Select(s =>
            {
                return new StockModel
                {
                    Date = s.Date,
                    DayHigh = s.DayHigh,
                    DayLow = s.DayLow,
                    DayOpen = s.DayOpen,
                    Price = s.Price,
                    Symbol = s.Symbol

                };
            }).ToArray();
            return new StockModels { Stocks = stocks };
        }
    }
}
