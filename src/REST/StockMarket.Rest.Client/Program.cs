using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockMarket.Rest.Client
{
    class Program
    {
        static async Task Main()
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

            using var response = await httpClient.GetAsync("StockMarket");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();

                var stocks = await JsonSerializer.DeserializeAsync<StockModels>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var stock in stocks.Stocks)
                {
                    PrintStockInfo(stock);
                }
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        static void PrintStockInfo(StockModel stock)
        {
            bool compare(string item1, string item2)
                => String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

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

            Console.WriteLine($"Symbol {stock.Symbol} - Date {stock.Date.ToString("MM/dd/yyyy")} - High Price {stock.DayHigh} - Low Price {stock.DayLow}");
            
            Console.ForegroundColor = color;
        }
    }
}
