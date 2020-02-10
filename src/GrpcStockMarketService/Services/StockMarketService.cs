using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockMarket.Grpc.Proto;
using StockMarket.Grpc;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using StockMarket.Grpc.Server.Helpers;
using StockGenerator = StockHistoryGenerator.StockMarket;

namespace StockMarket.Grpc.Server.Services
{
    public class StockMarketService : Proto.StockMarketService.StockMarketServiceBase
    {
        private readonly ILogger<StockMarketService> _logger;

        public StockMarketService(ILogger<StockMarketService> logger)
        {
            _logger = logger;
        }

        public Proto.Decimal FromDecimal(decimal value)
        {
            var units = decimal.ToInt64(value);
            var nanos = decimal.ToInt32((value - units) * 1_000_000_000);
            return new Proto.Decimal { Units = units, Nanos = nanos };
        }

        public override async Task<StockData> GetStock(StockRequest request, ServerCallContext context)
        {
            var opt = await StockGenerator.SearchStock(request.Symbol, request.Date.ToDateTime());            
            var stock = opt.Value;
           
            // return base.GetStock(request, context);
            return stock.ToStockData();            
        }

        public override async Task<StockHistoryReply> GetStockHistory(StockHistoryRequest request, ServerCallContext context)
        {
            var opt = await StockGenerator.RetrieveStockHistory(request.Symbol);

            // return base.GetStockHistory(request, context);
            return null;
        }

        public override Task GetStockMarketStream(Empty request, IServerStreamWriter<StockData> responseStream, ServerCallContext context)
        {
            
            return base.GetStockMarketStream(request, responseStream, context);
        }

        public override Task GetStockStream(IAsyncStreamReader<StockRequest> requestStream, IServerStreamWriter<StockReply> responseStream, ServerCallContext context)
        {
            return base.GetStockStream(requestStream, responseStream, context);
        }

    }
}
