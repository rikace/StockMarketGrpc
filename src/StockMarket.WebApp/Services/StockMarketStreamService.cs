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
using Channel = System.Threading.Channels.Channel;
using static StockHistoryGenerator.StockMarket;

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
            var channel = GrpcChannel.ForAddress("https://localhost:5005");
            return new StockMarketServiceClient(channel);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var client = Create();

                using var replies = client.GetStockMarketStream(new Empty(), cancellationToken: stoppingToken);

                try
                {
                    await foreach (var stockData in replies.ResponseStream.ReadAllAsync())
                    {
                        var stock = ToStock(stockData);
                        PrintStockInfo(stock);

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
            {
                Symbol = stockData.Symbol,
                Date = stockData.Date.ToDateTime(),
                DayHigh = ToDecimal(stockData.DayHigh),
                DayLow = ToDecimal(stockData.DayLow),
                DayOpen = ToDecimal(stockData.DayOpen),
                LastChange = ToDecimal(stockData.LastChange),
                Price = ToDecimal(stockData.Price),
            };
            return stock;
        }

        public static decimal ToDecimal(Grpc.Proto.Decimal value) => value.Units + value.Nanos / 1_000_000_000;

        static void PrintStockInfo(Stock stock)
        {
            bool compare(string item1, string item2)
                =>
                String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

            var symbol = stock.Symbol;

            var color = Console.ForegroundColor;
            if (compare(symbol, "MSFT"))
                Console.ForegroundColor = ConsoleColor.Green;
            else if (compare(symbol, "FB"))
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (compare(symbol, "AAPL"))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (compare(symbol, "GOOG"))
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (compare(symbol, "AMZN"))
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{stock.Symbol} |\t\t Date {stock.Date.ToString("MM/dd/yyyy")} | Higher price {stock.DayHigh} | Lower price {stock.DayLow}");
            Console.ForegroundColor = color;
        }
    }
}


