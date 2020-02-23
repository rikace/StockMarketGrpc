using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static StockHistoryGenerator.StockMarket;
using System.Threading.Channels;

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


    //public class DispatchStocks : IDispatchMessage<Stock>
    //{
    //    private readonly ConcurrentDictionary<string, Channel<Stock>> _channels = new ConcurrentDictionary<string, Channel<Stock>>();

    //    public async Task Dispatch(Stock message)
    //    {
    //        foreach (var channel in _channels)
    //        {
    //            await channel.Value.Writer.WriteAsync(message);
    //        }
    //    }

    //    public ChannelReader<Stock> Subscribe(string id)
    //    {
    //        return _channels.GetOrAdd(id, Create);
    //    }

    //    private static Channel<Stock> Create(string _)
    //    {
    //        return Channel.CreateBounded<Stock>(new BoundedChannelOptions(32)
    //        {
    //            FullMode = BoundedChannelFullMode.DropOldest,
    //            AllowSynchronousContinuations = false,
    //            SingleReader = false,
    //            SingleWriter = false
    //        });
    //    }
    //}
}
