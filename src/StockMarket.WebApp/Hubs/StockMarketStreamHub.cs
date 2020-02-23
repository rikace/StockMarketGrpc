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
using static StockHistoryGenerator.StockMarket;
using StockMarket.WebApp.Services;

namespace StockMarket.WebApp.Hubs
{
    public class StockMarketStreamHub : Hub
    {
        private readonly StockMarketServiceClient _client;
        private readonly IDispatchMessage<Stock> _dispatchMessage;

        public StockMarketStreamHub(StockMarketServiceClient client, IDispatchMessage<Stock> dispatchMessage)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _client = client;
            this._dispatchMessage = dispatchMessage;

            //_client = GrpcClient.Create<StockMarketService.StockMarketServiceClient>(httpClient);
        }

        public override Task OnConnectedAsync()
        {
            //_dispatchMessage.Subscribe(this.Context.ConnectionId);
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
                    await foreach (var stock in _dispatchMessage.Channel.ReadAllAsync())
                    {
                        string message = $"Symbol {stock.Symbol} |\t\t Date {stock.Date.ToString("MM/dd/yyyy")} | Higher price {stock.DayHigh} | Lower price {stock.DayLow}";
                        await writer.WriteAsync(message, cancellationToken);

                        PrintStockInfo(stock);
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


        //public ChannelReader<string> StockMarketStream(CancellationToken cancellationToken)
        //{
        //    var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();

        //    _ = WriteItemsAsync(channel.Writer);

        //    return channel.Reader;

        //    async Task WriteItemsAsync(ChannelWriter<string> writer)
        //    {
        //        using var replies = _client.GetStockMarketStream(new Empty(), cancellationToken: cancellationToken);

        //        try
        //        {
        //            await foreach (var stockData in replies.ResponseStream.ReadAllAsync())
        //            {
        //                var stock = ToStock(stockData);
        //                string message = $"Symbol {stock.Symbol} |\t\t Date {stock.Date.ToString("MM/dd/yyyy")} | Higher price {stock.DayHigh} | Lower price {stock.DayLow}";
        //                await writer.WriteAsync(message, cancellationToken);

        //                PrintStockInfo(stock);
        //            }
        //        }
        //        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        //        {
        //            replies.Dispose();
        //            Console.WriteLine("Stream cancelled.");
        //        }
        //        catch (Exception ex)
        //        {
        //            writer.TryComplete(ex);
        //        }

        //        writer.TryComplete();
        //    }
        //}

        public static decimal ToDecimal(Grpc.Proto.Decimal value)
            => value.Units + value.Nanos / 1_000_000_000;


        static void PrintStockInfo(Stock stock)
        {
            bool compare(string item1, string item2)
                =>
                String.Compare(item1, item2, StringComparison.OrdinalIgnoreCase) == 0;

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

            Console.WriteLine($"{stock.Symbol} |\t\t Date {stock.Date.ToString("MM/dd/yyyy")} | Higher price {stock.DayHigh} | Lower price {stock.DayLow}");
            Console.ForegroundColor = color;
        }

        static Stock ToStock(StockMarket.Grpc.Proto.StockData stockData)
        {
            var stock = new Stock
            {
                Symbol = stockData.Symbol,
                Date = stockData.Date.ToDateTime(),
                DayHigh = ToDecimal(stockData.DayHigh),
                DayLow = ToDecimal(stockData.DayLow),
                DayOpen = ToDecimal(stockData.DayOpen),
                LastChange = ToDecimal(stockData.LastChange),
                Price = ToDecimal(stockData.Price),
            };
            return stock;
        }
    }
}