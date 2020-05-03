using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using StockMarket.Common;

namespace StockMarket.TickerGenerator
{
    public class CsvStockStream
    {
        public CsvStockStream(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _tickerName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            
            GetRecordsAsyncInstance = new AsyncLazy<List<Stock>>(async () =>
            {
                var _data = new List<Stock>();    
                using (var stream = File.OpenRead(_fileInfo.FullName))
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();

                        var value = Stock.Parse(_tickerName, line);
                        if (value != null)
                        {
                            _data.Add(value);
                        }
                    }
                }

                _data.Reverse();
                return _data;
            });
        }

        private FileInfo _fileInfo;
        private string _tickerName;

        private AsyncLazy<List<Stock>> GetRecordsAsyncInstance { get; }
      

        private static int _index = 0;
        public async Task<Stock> Next()
        {
            var records = await GetRecordsAsyncInstance.Value;
            var stock = records[Thread.VolatileRead(ref _index) % records.Count];
            Interlocked.Increment(ref _index);
            return stock;
        }
        
        public string TickerName => _tickerName;
    }
    
    public class AsyncLazy<T> : Lazy<Task<T>> 
    { 
        public AsyncLazy(Func<T> valueFactory) : 
            base(() => Task.Factory.StartNew(valueFactory)) { }

        public AsyncLazy(Func<Task<T>> taskFactory) : 
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap()) { }

        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); } 
    }
}