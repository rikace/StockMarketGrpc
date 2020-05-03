using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using static StockMarket.Grpc.Proto.StockMarketService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using StockMarket.Common;
using StockMarket.Grpc.Proto;
using Channel = System.Threading.Channels.Channel;

namespace StockMarket.WebApp.Services
{
    public class StockMarketStreamService : BackgroundService
    {
        private readonly IDispatchMessage<Stock> dispatchMessage;
        private readonly ILogger<StockMarketStreamService> logger;

        public StockMarketStreamService(ILogger<StockMarketStreamService> logger, IDispatchMessage<Stock> dispatchMessage)
        {
            this.logger = logger;
            this.dispatchMessage = dispatchMessage;
        }

        static StockMarketServiceClient Create()
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5000");
            return new StockMarketServiceClient(channel);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var client = Create();

                using var replies = client.GetStockMarketStream(new StockStreamRequest(), cancellationToken: stoppingToken);

                try
                {
                    await foreach (var stockData in replies.ResponseStream.ReadAllAsync())
                    {
                        var stock = ToStock(stockData);
                        stock.PrintStockInfo();

                        await dispatchMessage.Dispatch(stock);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    replies.Dispose();
                    Console.WriteLine("Stream cancelled.");
                }
                catch (Exception)
                {
                }
            }
        }

        static Stock ToStock(StockMarket.Grpc.Proto.StockData stockData)
        {
            var stock = new Stock
            (
                stockData.Symbol,
                stockData.Date.ToDateTime(),
                ToDecimal(stockData.DayOpen),
                ToDecimal(stockData.DayHigh),
                ToDecimal(stockData.DayLow),
                ToDecimal(stockData.DayClose)
            );
            return stock;
        }
        
        // TODO
        private static decimal ToDecimal(Grpc.Proto.Decimal value) => value.Units + value.Nanos / 1_000_000_000;
    }
}


