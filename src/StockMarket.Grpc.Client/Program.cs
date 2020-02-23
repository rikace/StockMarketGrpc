using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
        static StockMarketServiceClient Create(string url)
        {
            var cert = new X509Certificate2("cert_fleName", "cert_password");

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler);
            var opt = new GrpcChannelOptions { HttpClient = client };

            var channel = GrpcChannel.ForAddress(url, opt);
            return new StockMarketServiceClient(channel);
        }

        private static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5005");

            var client = new StockMarketServiceClient(channel);

            while (true)
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
                    PrintStockInfo(stockData);
                }
            }

            Console.ReadKey();
        }

        public static decimal ToDecimal(Proto.Decimal value)

        {
            return value.Units + value.Nanos / 1_000_000_000;
        }

        static void PrintStockInfo(StockData stockData)
        {
            bool compare(string item1, string item2)
                => String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

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

            Console.WriteLine($"Symbol {stockData.Symbol} - Date {stockData.Date.ToDateTime().ToString("MM/dd/yyyy")} - High Price {ToDecimal(stockData.DayHigh)} - Low Price {ToDecimal(stockData.DayLow)}");

            Console.ForegroundColor = color;
        }
    }
}
