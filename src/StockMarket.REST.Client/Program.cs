using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using StockMarket.REST.Server.Models;

namespace StockMarket.REST.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            // await FetchStocks();
            await FetchStocksWithOffset();

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

        static async Task FetchStocks()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using var httpClient = new HttpClient(clientHandler) { BaseAddress = new Uri("http://localhost:5000")};

            while (true)
            {
                Console.WriteLine("Symbol: ");
                string symbol = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(symbol))
                    break;

                using var response = await httpClient.GetAsync($"StockMarket/Get/{symbol}");

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
            }
        }
        
        static async Task FetchStocksWithOffset()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using var httpClient = new HttpClient(clientHandler) { BaseAddress = new Uri("http://localhost:5000")};

            while (true)
            {
                Console.WriteLine("Symbol: ");
                string symbol = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(symbol))
                    break;

                int offset = 0;
                int count = 10;

                while (offset < 100)
                {
                    Console.WriteLine($"Fetching stocks for ticker {symbol} - offset {offset} count {count}");
                    
                    using var response =
                        await httpClient.GetAsync($"StockMarket/GetPagination/{symbol}/{offset}/{count}");

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

                    await Task.Delay(200);

                    offset += count;
                }
            }
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

            Console.WriteLine(
                $"Symbol {stock.Symbol} - Date {stock.Date:MM/dd/yyyy} - High Price {stock.DayHigh} - Low Price {stock.DayLow}");

            Console.ForegroundColor = color;
        }
    }
}
