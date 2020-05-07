using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using StockMarket.WebApp.Services;
using StockMarket.Grpc.Proto;
using static StockMarket.Grpc.Proto.StockMarketService;
using StockMarket.Common;
using Decimal = StockMarket.Grpc.Proto.Decimal;

namespace StockMarket.WebApp.Hubs
{
    public class StockMarketStreamHub : Hub
    {
        private readonly StockMarketServiceClient _client;
        private readonly IDispatchMessage<Stock> _dispatchMessage;

        public StockMarketStreamHub(StockMarketServiceClient client, IDispatchMessage<Stock> dispatchMessage)
        {
            _client = client;
            this._dispatchMessage = dispatchMessage;
        }

        public ChannelReader<string> StockMarketStream(CancellationToken cancellationToken)
        {
            var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();

            _ = WriteItemsAsync(channel.Writer);

            return channel.Reader;

            async Task WriteItemsAsync(ChannelWriter<string> writer)
            {
                try
                { 
                    using var replies = _client.GetStockMarketStream(new StockStreamRequest(), cancellationToken: cancellationToken);
                    
                    await foreach (var stock in replies.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        string message =
                            $"Symbol {stock.Symbol} |\t\t Date {stock.Date:MM/dd/yyyy} | Higher price {ToDecimal(stock.DayHigh)}";
                        await writer.WriteAsync(message, cancellationToken);

                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Stream cancelled.");
                }
                catch (Exception ex)
                {
                    writer.TryComplete(ex);
                }

                writer.TryComplete();
            }
        }
        
        private static decimal ToDecimal(Decimal value)
        {
            return value.Units + value.Nanos / 1_000_000_000;
        }

    }
}