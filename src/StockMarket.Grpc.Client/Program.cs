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

            while(true)
            {
                Console.WriteLine("Specify Symbol to retrieve history");
                string symbol = Console.ReadLine();
                if (symbol == "q")
                    break;

                var reply = client.GetStockHistory(new StockHistoryRequest
                {
                    Symbol = symbol
                });

                foreach (var stockData in reply.StockData)
                {
                   Console.WriteLine($"Symbol {stockData.Symbol} - Date {stockData.Date.ToDateTime().ToString("MM/dd/yyyy")} - High Price {ToDecimal (stockData.DayHigh)} - Low Price {ToDecimal (stockData.DayLow)}");
                }
            }

            Console.ReadKey();
        }

        public static decimal ToDecimal(Proto.Decimal value)

        {
            return value.Units + value.Nanos / 1_000_000_000;
        }
    }
}
