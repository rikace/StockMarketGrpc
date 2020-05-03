using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using StockMarket.Grpc.Proto;

namespace StockMarket.Grpc.Client
{
    class Program
    {
        static StockMarketService.StockMarketServiceClient Create(string url)
        {
            var cert = new X509Certificate2("cert_fleName", "cert_password");

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler);
            var opt = new GrpcChannelOptions { HttpClient = client };

            var channel = GrpcChannel.ForAddress(url, opt);
            return new StockMarketService.StockMarketServiceClient(channel);
        }

        private static async Task Main()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new StockMarketService.StockMarketServiceClient(channel);

            await GetStockHistoryAsync(client);
            
            Console.ReadKey();
        }

        static async Task GetStockHistoryAsync(StockMarketService.StockMarketServiceClient client)
        {
            
            while (true)
            {
                Console.WriteLine("Specify Symbol to retrieve history");
                string symbol = Console.ReadLine();
                if (symbol == "q")
                    break;

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                
                var reply = await client.GetStockHistoryAsync(new StockHistoryRequest
                {
                    Symbol = symbol
                }, cancellationToken: cts.Token);

                foreach (var stockData in reply.StockData)
                {
                    PrintStockInfo(stockData);
                }
            }
        }


        
        private static decimal ToDecimal(Proto.Decimal value)

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