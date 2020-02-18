using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using StockMarket.Grpc;
using StockMarket.Grpc.Proto;
using static StockMarket.Grpc.Proto.StockMarketService;

namespace StockMarket.Grpc.ClientServerStreaming
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5005");
            var client = new StockMarketServiceClient(channel);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            //ing var replies = client.GetStockMarketStream(new Empty(), cancellationToken: cts.Token);

            using var stockStreamService = client.GetStockStream();

            var responseProcessing = Task.Run(async () =>
            {
                try
                {
                    await foreach (var stockReply in stockStreamService.ResponseStream.ReadAllAsync())
                    {                       
                        PrintStockInfo(stockReply);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Stream cancelled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading response: " + ex);
                }
            });

            var datesUtc = Dates.Select(datesUtc => datesUtc.ToUniversalTime()).ToArray();

            for (int i = 0; i < 5; i++)
            {
                foreach (var symbol in new[] { "AAPL", "AMZN", "FB", "GOOG", "MSFT" })
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Requesting stock info for {symbol}...");
                    Console.ResetColor();

                    int index = rnd.Next(Dates.Count);
                    var date = datesUtc[index].ToTimestamp();
                    Console.WriteLine($"Requesting Stock {symbol} history for date {date.ToDateTime().ToString("MM/dd/yyyy")}...");

                    await stockStreamService.RequestStream.WriteAsync(new StockRequest
                    {
                        Symbol = symbol,
                        Date = date
                    });

                    await Task.Delay(2500); // simulate delay getting next item
                }
            }

            Console.WriteLine("Completing request stream");
            await stockStreamService.RequestStream.CompleteAsync();
            Console.WriteLine("Request stream completed");

            await responseProcessing;

            Console.WriteLine("Read all responses");
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();

        }

        static void PrintStockInfo(StockPerDateReply stockReply)
        {
            bool compare(string item1, string item2)
                =>
                String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

            var symbol = stockReply.Symbol;

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

            Console.WriteLine($"Request for Stock {stockReply.Symbol} received...");

            Console.WriteLine($"Symbol {stockReply.Symbol} - Date {stockReply.Date.ToDateTime().ToString("MM/dd/yyyy")} - High Price {ToDecimal (stockReply.BestPrice)} - Low Price {ToDecimal (stockReply.WorstPrice)}");
            Console.ForegroundColor = color;
        }

        static Random rnd = new Random();

        static List<DateTime> Dates = new List<DateTime>
        {
            // year month day
           new DateTime(2017,5,31),
           new DateTime(2017,5,11),
           new DateTime(2016,8,31),
           new DateTime(2017,7,31),
           new DateTime(2017,7,28),
           new DateTime(2017,7,27),
           new DateTime(2017,7,26),
           new DateTime(2017,7,25),
           new DateTime(2017,7,24),
           new DateTime(2017,7,21),
           new DateTime(2017,7,20),
           new DateTime(2017,7,19),
           new DateTime(2017,7,18),
           new DateTime(2017,7,17),
           new DateTime(2017,7,14),
           new DateTime(2017,7,13),
           new DateTime(2017,7,12),
           new DateTime(2017,7,11),
           new DateTime(2017,7,10),
           new DateTime(2017,4,28),
           new DateTime(2017,4,27),
           new DateTime(2017,4,26),
           new DateTime(2017,4,25),
           new DateTime(2017,4,24),
           new DateTime(2017,4,21),
           new DateTime(2017,4,20),
           new DateTime(2017,4,19),
           new DateTime(2017,4,18),
           new DateTime(2017,4,17),
           new DateTime(2017,4,13),
           new DateTime(2017,4,12),
           new DateTime(2017,4,11),
           new DateTime(2017,4,10),
           new DateTime(2016,10,31),
           new DateTime(2016,10,28),
           new DateTime(2016,10,27),
           new DateTime(2016,10,26),
           new DateTime(2016,10,25),
           new DateTime(2016,10,24),
           new DateTime(2016,10,21),
           new DateTime(2016,10,20),
           new DateTime(2016,10,19),
           new DateTime(2016,10,18),
           new DateTime(2016,10,17),
           new DateTime(2016,10,14),
           new DateTime(2016,10,13),
           new DateTime(2016,10,12),
           new DateTime(2016,10,11),
           new DateTime(2016,10,10),
           new DateTime(2016,6,30),
           new DateTime(2016,6,29),
           new DateTime(2016,6,28),
           new DateTime(2016,6,27),
           new DateTime(2016,6,24),
           new DateTime(2016,6,23),
           new DateTime(2016,6,22),
           new DateTime(2016,6,21),
           new DateTime(2016,6,20),
           new DateTime(2016,6,17),
           new DateTime(2016,6,16),
           new DateTime(2016,6,15),
           new DateTime(2016,6,14),
           new DateTime(2016,6,13),
           new DateTime(2016,6,10)
        };

        private const decimal NanoFactor = 1_000_000_000;


        public static decimal ToDecimal(Proto.Decimal value)
            => value.Units + value.Nanos / NanoFactor;
    }
}
