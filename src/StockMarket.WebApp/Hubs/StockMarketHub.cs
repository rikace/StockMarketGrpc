using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using StockMarket.WebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

using StockMarket.Grpc;
using StockMarket.Grpc.Proto;
using static StockMarket.Grpc.Proto.StockMarketService;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using Grpc.Core;
using StockMarket.Common;
using StockMarket.WebApp.Services;

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

        public override Task OnConnectedAsync()
        {
            // _dispatchMessage.Subscribe(this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            //_dispatchMessage.Unsubscribe(this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
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
                    //await foreach (var stock in _dispatchMessage.Channel.ReadAllAsync(cancellationToken))
                    {
                        string message =
                            $"Symbol {stock.Symbol} |\t\t Date {stock.Date:MM/dd/yyyy} | Higher price {stock.DayHigh} | Lower price {stock.DayHigh}";
                        await writer.WriteAsync(message, cancellationToken);

                       // stock.PrintStockInfo();
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
    }
}