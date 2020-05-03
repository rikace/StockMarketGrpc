using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StockMarket.Common;

namespace StockMarket.TickerGenerator
{
    class Program
    {
        private static ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());
        
        static async Task Main(string[] args)
        {
            var tickersDirectoryPath = "../Data/Tickers";

            while (!Directory.Exists(tickersDirectoryPath))
            {
                tickersDirectoryPath = "../" + tickersDirectoryPath;
            }
            
            
            
            var stockData = 
                Directory.GetFiles(tickersDirectoryPath, "*.csv")
                .Select(file => new CsvStockStream(new FileInfo(file)))
                .ToDictionary(k => k.TickerName, v => v);
            
            var  stocks = stockData.Keys.ToArray();

            while (true)
            {
                var index = _random.Value.Next(stockData.Count);
                var stockStream = stockData[stocks[index]];

                var stock = await stockStream.Next();

                stock.PrintStockInfo();

                await Task.Delay(1000);
            }
        }
    }
}