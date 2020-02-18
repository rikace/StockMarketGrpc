using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockMarket.Grpc.Proto;
using StockMarket.Grpc;
using FSharp.Control;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using StockMarket.Grpc.Server.Helpers;
using StockGenerator = StockHistoryGenerator.StockMarket;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

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
            var stock = await StockGenerator.SearchStock(request.Symbol, request.Date.ToDateTime());

            await Task.Delay(2000); // Gotta look busy            
            return stock.ToStockData();          
        }

        public override async Task<StockHistoryReply> GetStockHistory(StockHistoryRequest request, ServerCallContext context)
        {
            var stocks = await StockGenerator.RetrieveStockHistory(request.Symbol);
            var stockdata = stocks.Select(StockConverter.ToStockData);

            await Task.Delay(2000); // Gotta look busy

            var stockHistoryReply = new StockHistoryReply
            {
                StockData = { stockdata }
            };

            return stockHistoryReply;
        }

        public override async Task GetStockMarketStream(Empty request, IServerStreamWriter<StockData> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var actionBlock = new ActionBlock<StockGenerator.Stock>(async stock =>
                {
                    _logger.LogInformation("Sending StockMarketStream response");

                    await responseStream.WriteAsync(stock.ToStockData());

                    //await Task.Delay(250); // Gotta look busy
                });


                var stream = StockGenerator.StockStream();
                stream
                    .Delay(TimeSpan.FromMilliseconds(100))
                    .Subscribe(actionBlock.AsObserver());


                //stream.Select(stock =>
                //    Observable.FromAsync(async () =>
                //    {
                //        _logger.LogInformation("Sending StockMarketStream response");

                //        await responseStream.WriteAsync(stock.ToStockData());

                //        await Task.Delay(250); // Gotta look busy
                //    })
                //    ).Concat().Subscribe();
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The client cancelled their request");
            }
            // return base.GetStockMarketStream(request, responseStream, context);
        }

        public override async Task GetStockStream(IAsyncStreamReader<StockRequest> requestStream,
            IServerStreamWriter<StockPerDateReply> responseStream, ServerCallContext context)
        {
            // we'll use a channel here to handle in-process 'messages' 
            // concurrently being written to and read from the channel.
            var channel = Channel.CreateUnbounded<StockPerDateReply>();

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
                var stock = await StockGenerator.SearchStock(symbol, date);
                await Task.Delay(500); // Gotta look busy        

                // write the forecast to the channel which will be picked up concurrently by the channel reading background task
                await channel.Writer.WriteAsync(
                        new StockPerDateReply
                        {
                            Symbol = stock.Symbol,
                            Date = stock.Date.ToTimestamp(),
                            BestPrice = stock.DayHigh.FromDecimal(),
                            WorstPrice = stock.DayLow.FromDecimal(),
                        });
            }
        }
        //return base.GetStockStream(requestStream, responseStream, context);
    }
}