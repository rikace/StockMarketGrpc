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
using System.Reactive.Linq;
using System.Threading.Channels;

namespace StockMarket.Grpc.Server.Services
{
    public class StockMarketService : Proto.StockMarketService.StockMarketServiceBase
    {
        private readonly ILogger<StockMarketService> _logger;

        public StockMarketService(ILogger<StockMarketService> logger)
        {
            _logger = logger;
        }
               
        public override async Task<StockData> GetStock(StockRequest request, ServerCallContext context)
        {
            var opt = await StockGenerator.SearchStock(request.Symbol, request.Date.ToDateTime());
            var stock = opt.Value;
            await Task.Delay(2000); // Gotta look busy

            // return base.GetStock(request, context);
            return stock.ToStockData();
        }

        public override async Task<StockHistoryReply> GetStockHistory(StockHistoryRequest request, ServerCallContext context)
        {
            var opt = await StockGenerator.RetrieveStockHistory(request.Symbol);
            var stockdata = opt.Select(StockConverter.ToStockData);

            await Task.Delay(2000); // Gotta look busy

            var stockHistoryReply = new StockHistoryReply
            {
                StockData = { stockdata }
            };

            // return base.GetStockHistory(request, context);
            return stockHistoryReply;
        }

        public override async Task GetStockMarketStream(Empty request, IServerStreamWriter<StockData> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var stream = StockGenerator.StcokStream();
                stream.Select(stock =>
                    Observable.FromAsync(async () => await responseStream.WriteAsync(stock.ToStockData()))
                    ).Concat().Subscribe();

                _logger.LogInformation("Sending StockMarketStream response");

                await Task.Delay(500); // Gotta look busy
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The client cancelled their request");
            }
            // return base.GetStockMarketStream(request, responseStream, context);
        }

        public override async Task GetStockStream(IAsyncStreamReader<StockRequest> requestStream,
            IServerStreamWriter<StockReply> responseStream, ServerCallContext context)
        {
            // we'll use a channel here to handle in-process 'messages' 
            // concurrently being written to and read from the channel.
            var channel = Channel.CreateUnbounded<StockReply>();

            // background task which uses async streams to write each forecast from the channel to the response steam.
            _ = Task.Run(async () =>
            {
                await foreach (var stock in channel.Reader.ReadAllAsync())
                {
                    await responseStream.WriteAsync(stock);
                }
            });

            // a list of tasks handling requests concurrently
            var getStockRequestTasks = new List<Task>();

            try
            {
                // async streams used to process each request from the stream as they are receieved
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Getting stock-data for {request.Symbol}");
                    // start and add the request handling task
                    getStockRequestTasks.Add(GetStockDataAsync(request.Symbol));
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
            async Task GetStockDataAsync(string symbol)
            {

                var stock = await StockGenerator.StockPriceRangeHistory(symbol);
                await Task.Delay(500); // Gotta look busy        

                // write the forecast to the channel which will be picked up concurrently by the channel reading background task

                await channel.Writer.WriteAsync(
                        new StockReply
                        {
                            Symbol = stock.Symbol,
                            BestDate = stock.BestDate.ToTimestamp(),
                            BestPrice = stock.BestPrice.FromDecimal(),
                            WorstPrice = stock.WorstPrice.FromDecimal(),
                            WorstDate = stock.WorstDate.ToTimestamp()
                        });
            }
        }
        //return base.GetStockStream(requestStream, responseStream, context);
    }
}
