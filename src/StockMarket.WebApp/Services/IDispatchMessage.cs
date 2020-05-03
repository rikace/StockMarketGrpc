using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using static StockHistoryGenerator.StockMarket;
using System.Threading.Channels;
using StockMarket.Common;

namespace StockMarket.WebApp.Services
{
    public interface IDispatchMessage<T> where T : class
    {
        Task Dispatch(T message);
        ChannelReader<T> Channel { get; }
    }

    public class DispatchStocks : IDispatchMessage<Stock>
    {
        private readonly Channel<Stock> _channel;

        public DispatchStocks()
        {
            _channel = Create();
        }

        public async Task Dispatch(Stock message)
                => await _channel.Writer.WriteAsync(message);           

        public ChannelReader<Stock> Channel => _channel.Reader;

        private static Channel<Stock> Create(int capacity = 32)
        {
            return System.Threading.Channels.Channel
                .CreateBounded<Stock>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                AllowSynchronousContinuations = false,
                SingleReader = false,
                SingleWriter = false
            });
        }
    }
}
