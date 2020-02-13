using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using StockMarket.Grpc;
using StockMarket.Grpc.Proto;
using static StockMarket.Grpc.Proto.StockMarketService;

namespace StockMarket.Grpc.Client
{
    class Program
    {
        private static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5005");

            var client = new StockMarketServiceClient(channel);

            string symbol = "";
            while(symbol != null)
            {
                Console.WriteLine("Specify Symbol to retrieve history");
                symbol = Console.ReadLine();
                if (symbol == "q")
                    break;

                var reply = client.GetStockHistory(new StockHistoryRequest
                {
                    Symbol = symbol
                });

                foreach (var stockData in reply.StockData)
                {
                   Console.WriteLine($"Date {stockData.Date.ToDateTime():s} - High Price {stockData.DayHigh} - Low Price {stockData.DayLow}");
                }
            }

            Console.ReadKey();
        }
    }
}
