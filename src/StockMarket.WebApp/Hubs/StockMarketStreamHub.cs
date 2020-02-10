using Grpc.Core;
//using StockMarketGrpc;

namespace StockMarket.WebApp.Hubs
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using StockMarket.Grpc.Proto;
    using Microsoft.AspNetCore.SignalR;


    public class StockMarketStreamHub : Hub
    {
        private readonly StockMarketService.StockMarketServiceClient _client;

        public StockMarketStreamHub(StockMarketService.StockMarketServiceClient client)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _client = client;

        //_client = GrpcClient.Create<StockMarketService.StockMarketServiceClient>(httpClient);
        }

        // public ChannelReader<string> StockMarketStream(CancellationToken cancellationToken)
        // {
        //     var channel = Channel.CreateUnbounded<string>();
        //
        //     _ = WriteItemsAsync(channel.Writer);
        //
        //     return channel.Reader;
        //
        //     async Task WriteItemsAsync(ChannelWriter<string> writer)
        //     {
        //         using var replies = _client.GetWeatherStream(new WeatherRequest(), cancellationToken: cancellationToken);
        //
        //         try
        //         {
        //             await foreach(var forecast in replies.ResponseStream.ReadAllAsync())
        //             {                  
        //                 var date = DateTimeOffset.FromUnixTimeSeconds(forecast.DateTimeStamp);
        //                 await writer.WriteAsync($"{date:s} | {forecast.Summary} | {forecast.TemperatureC} C", cancellationToken);
        //             }
        //         }
        //         catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        //         {
        //             replies.Dispose();
        //             Console.WriteLine("Stream cancelled.");
        //         }
        //         catch (Exception ex)
        //         {
        //             writer.TryComplete(ex);
        //         }
        //
        //         writer.TryComplete();
        //     }
        // }
    }
}