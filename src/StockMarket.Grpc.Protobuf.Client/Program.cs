using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using StockMarket.Grpc.Shared;

namespace StockMarket.Grpc.Protobuf.Client
{
    class Program
    {
          private static async Task Main()
        {

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using (var channel = GrpcChannel.ForAddress("http://localhost:5012"))
            {
                var stockMarketService = channel.CreateGrpcService<IStockMarketService>();

                while (true)
                {
                    Console.WriteLine("Symbol: ");
                    string symbol = Console.ReadLine();
                    if (String.IsNullOrWhiteSpace(symbol))
                        break;

                    var request = new StockRequest { Symbol = symbol };
                    var stockResult = await stockMarketService.GetStockAsync(request);


                    foreach (var stock in stockResult.Stocks)
                    {
                        PrintStockInfo(stock);
                    }
                }
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        static void PrintStockInfo(StockData stockData)
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

            Console.WriteLine($"Symbol {stockData.Symbol} - Date {stockData.Date.ToString("MM/dd/yyyy")} - High Price {stockData.DayHigh} - Low Price {stockData.DayLow}");
            Console.ForegroundColor = color;
        }
    }
}
