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
                        var date =  stockReply.BestDate.ToDateTime();

                        //Console.WriteLine($"{forecast.TownName} = {date:s} | {forecast.WeatherData.Summary} | {forecast.WeatherData.TemperatureC} C");
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

            foreach (var symbol in new[] { "APPL", "AMZN", "FB", "GOOG", "MSFT" })
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Requesting stock info for {symbol}...");
                Console.ResetColor();

                int index  = rnd.Next(Dates.Count);

                await stockStreamService.RequestStream.WriteAsync(new StockRequest
                {
                    Symbol = symbol,
                    Date = datesUtc[index].ToTimestamp()
                });

                await Task.Delay(2500); // simulate delay getting next item
            }

            Console.WriteLine("Completing request stream");
            await stockStreamService.RequestStream.CompleteAsync();
            Console.WriteLine("Request stream completed");

            await responseProcessing;

            Console.WriteLine("Read all responses");
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();

        }

        static void PrintStockInfo(StockReply stockData)
        {
            bool compare(string item1, string item2)
                =>
                String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

            var symbol = stockData.Symbol;

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

            Console.WriteLine($"Symbol {stockData.Symbol} - Date {stockData.Date.ToDateTime():s} - High Price {stockData.DayHigh} - Low Price {stockData.DayLow}");
            Console.ForegroundColor = color;
        }

        static Random rnd = new Random();

        static List<DateTime> Dates = new List<DateTime>
        {
            // year month day
           new DateTime(17,5,31 ),
           new DateTime(17,5,11 ),
           new DateTime(16,8,31 ),
           new DateTime(17,7,31 ),
           new DateTime(17,7,28 ),
           new DateTime(17,7,27 ),
           new DateTime(17,7,26 ),
           new DateTime(17,7,25 ),
           new DateTime(17,7,24 ),
           new DateTime(17,7,21 ),
           new DateTime(17,7,20 ),
           new DateTime(17,7,19 ),
           new DateTime(17,7,18 ),
           new DateTime(17,7,17 ),
           new DateTime(17,7,14 ),
           new DateTime(17,7,13 ),
           new DateTime(17,7,12 ),
           new DateTime(17,7,11 ),
           new DateTime(17,7,10 ),
           new DateTime(17,4,28 ),
           new DateTime(17,4,27 ),
           new DateTime(17,4,26 ),
           new DateTime(17,4,25 ),
           new DateTime(17,4,24 ),
           new DateTime(17,4,21 ),
           new DateTime(17,4,20 ),
           new DateTime(17,4,19 ),
           new DateTime(17,4,18 ),
           new DateTime(17,4,17 ),
           new DateTime(17,4,13 ),
           new DateTime(17,4,12 ),
           new DateTime(17,4,11 ),
           new DateTime(17,4,10 ),
           new DateTime(16,10,31 ),
           new DateTime(16,10,28 ),
           new DateTime(16,10,27 ),
           new DateTime(16,10,26 ),
           new DateTime(16,10,25 ),
           new DateTime(16,10,24 ),
           new DateTime(16,10,21 ),
           new DateTime(16,10,20 ),
           new DateTime(16,10,19 ),
           new DateTime(16,10,18 ),
           new DateTime(16,10,17 ),
           new DateTime(16,10,14 ),
           new DateTime(16,10,13 ),
           new DateTime(16,10,12 ),
           new DateTime(16,10,11 ),
           new DateTime(16,10,10 ),
           new DateTime(16,6,30 ),
           new DateTime(16,6,29 ),
           new DateTime(16,6,28 ),
           new DateTime(16,6,27 ),
           new DateTime(16,6,24 ),
           new DateTime(16,6,23 ),
           new DateTime(16,6,22 ),
           new DateTime(16,6,21 ),
           new DateTime(16,6,20 ),
           new DateTime(16,6,17 ),
           new DateTime(16,6,16 ),
           new DateTime(16,6,15 ),
           new DateTime(16,6,14 ),
           new DateTime(16,6,13 ),
           new DateTime(16,6,10 )
        };


    }
}
