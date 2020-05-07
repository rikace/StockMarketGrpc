using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using StockMarket.Common;

namespace StockMarket.TickerGenerator
{
    public class StockService : IStockService
    {
        private Lazy<FileInfo[]> tickers =>
            new Lazy<FileInfo[]>(() =>
            {
                var tickersDirectoryPath = "../Data/Tickers";
                while (!Directory.Exists(tickersDirectoryPath))
                {
                    tickersDirectoryPath = "../" + tickersDirectoryPath;
                }

                return new DirectoryInfo(tickersDirectoryPath).GetFiles("*.csv");
            });

        private FileInfo FindTickerCSV(string ticker) =>
            tickers.Value.FirstOrDefault(f =>
                String.Compare(ticker, Path.GetFileNameWithoutExtension(f.Name), StringComparison.OrdinalIgnoreCase) ==
                0);


        public async IAsyncEnumerable<Stock> StockHistoryStream(string ticker,
            [EnumeratorCancellation] CancellationToken token)
        {
            var tickerFile = FindTickerCSV(ticker);
            var symbol = Path.GetFileNameWithoutExtension(tickerFile.Name).ToUpper();

            using (var stream = tickerFile.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    var stock = Stock.Parse(symbol, line);
                    await Task.Delay(100, token); // looks busy! 
                    yield return stock;
                }
            }
        }

        private static Dictionary<string, List<Stock>> _stockHistory = new Dictionary<string, List<Stock>>();

        public async Task<IEnumerable<Stock>> RetrieveStockHistory(string ticker, CancellationToken token)
        {
            if (_stockHistory.TryGetValue(ticker, out var stocks))
                return stocks;

            var tickerFile = FindTickerCSV(ticker);
            var symbol = Path.GetFileNameWithoutExtension(tickerFile.Name).ToUpper();
            var stockHistory = new List<Stock>();

            using (var stream = tickerFile.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    var stock = Stock.Parse(symbol, line);
                    if (stock != null)
                        stockHistory.Add(stock);
                }
            }

            _stockHistory.Add(ticker, stockHistory);
            return stockHistory;
        }

        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        public async IAsyncEnumerable<Stock> StockStream([EnumeratorCancellation] CancellationToken token)
        {
            var stockData =
                tickers.Value
                    .Select(file => new CsvStockStream((file)))
                    .ToDictionary(k => k.TickerName, v => v);

            var stocks = stockData.Keys.ToArray();

            while (!token.IsCancellationRequested)
            {
                var index = Random.Value.Next(stockData.Count);
                var stockStream = stockData[stocks[index]];

                var stock = await stockStream.Next();

                await Task.Delay(100, token); // looks busy!

                yield return stock;
            }
        }

        public async Task<Stock> SearchStock(string ticker, DateTime date, CancellationToken token)
        {
            var tickerFile = FindTickerCSV(ticker);
            var symbol = Path.GetFileNameWithoutExtension(tickerFile.Name).ToUpper();

            using (var stream = tickerFile.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    var stock = Stock.Parse(symbol, line);
                    if (stock != null) 
                        return stock;
                }
            }

            return null;
        }
    }
}