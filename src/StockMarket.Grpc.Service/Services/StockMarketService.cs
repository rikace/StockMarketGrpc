using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StockMarket.Grpc.Proto;
using Google.Protobuf.WellKnownTypes;
using StockMarket.Grpc.Service.Infrastructure;
using StockMarket.TickerGenerator;

using Channel = System.Threading.Channels.Channel;

namespace StockMarket.Grpc.Service
{
    public class StockMarketService : Proto.StockMarketService.StockMarketServiceBase
    {
        private readonly ILogger<StockMarketService> _logger;
        private readonly IStockService _stockService;

        public StockMarketService(ILogger<StockMarketService> logger, IStockService stockService)
        {
            _logger = logger;
            _stockService = stockService;
        }

        public override async Task<StockData> GetStock(StockRequest request, ServerCallContext context)
        {
            var token = context.CancellationToken;
            var stock = await _stockService.SearchStock(request.Symbol, request.Date.ToDateTime(), token);

            await Task.Delay(2000); // look busy            
            return stock.ToStockData();
        }

        public override async Task<StockHistoryReply> GetStockHistory(StockHistoryRequest request,
            ServerCallContext context)
        {
            var token = context.CancellationToken;
            var stocks = await _stockService.RetrieveStockHistory(request.Symbol, token);
            var stockdata = stocks.Select(StockConverter.ToStockData);

            await Task.Delay(200, token); // look busy

            var stockHistoryReply = new StockHistoryReply
            {
                StockData = {stockdata}
            };

            return stockHistoryReply;
        }

        public override async Task GetStockMarketStream(StockStreamRequest request,
            IServerStreamWriter<StockData> responseStream,
            ServerCallContext context)
        {
            var token = context.CancellationToken;

            _logger.LogInformation("Sending StockMarketStream response");

            await foreach (var stock in _stockService.StockStream(token))
            {
                await responseStream.WriteAsync(stock.ToStockData());
                await Task.Delay(250, token); // look busy
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The client cancelled their request");
            }
        }

        public override async Task GetStockStream(IAsyncStreamReader<StockRequest> requestStream,
            IServerStreamWriter<StockPerDateReply> responseStream, ServerCallContext context)
        {
            var token = context.CancellationToken;
            
            // we'll use a channel here to handle in-process 'messages' 
            // concurrently being written to and read from the channel.
            var channel = Channel.CreateUnbounded<StockPerDateReply>();

            // background task which uses async streams to write each forecast from the channel to the response steam.
            _ = Task.Run(async () =>
            {
                await foreach (var stock in channel.Reader.ReadAllAsync(token))
                {
                    await responseStream.WriteAsync(stock);
                }
            }, token);

            // a list of tasks handling requests concurrently
            var getStockRequestTasks = new List<Task>();

            try
            {
                // async streams used to process each request from the stream as they are receieved
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Getting stock-data for {request.Symbol}");
                    // start and add the request handling task
                    getStockRequestTasks.Add(GetStockDataAsync(request.Symbol, request.Date.ToDateTime()));
                }

                _logger.LogInformation("Client finished streaming");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred");
            }

            // wait for all responses to be written to the channel 
            // from the concurrent tasks handling each request
            await Task.WhenAll(getStockRequestTasks);

            channel.Writer.TryComplete();

            //  wait for all responses to be read from the channel and streamed as responses
            await channel.Reader.Completion;

            _logger.LogInformation("Completed response streaming");

            // a local function which defines a task to handle a town forecast request
            // it produces 10 forecasts for each town, simulating a 0.5s time to gather each forecast
            // multiple instances of this will run concurrently for each recieved request
            async Task GetStockDataAsync(string symbol, DateTime date)
            {
                var stock = await _stockService.SearchStock(symbol, date, token);
                await Task.Delay(150, token); // look busy        

                // write the forecast to the channel which will be picked up concurrently by the channel reading background task
                await channel.Writer.WriteAsync(
                    new StockPerDateReply
                    {
                        Symbol = stock.Symbol,
                        Date = stock.Date.ToTimestamp(),
                        BestPrice = stock.High.FromDecimal(),
                        WorstPrice = stock.Low.FromDecimal(),
                    }, token);
            }
        }
    }
}