using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.HealthCheck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockMarket.TickerGenerator;

namespace StockMarket.Grpc.Service
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddHealthChecks();
            services.AddSingleton<HealthServiceImpl>();
            
            // gRPC services have full access to the ASP.NET Core features such as Dependency Injection (DI) and Logging.
            // For example, the service implementation can resolve a logger service from the DI container via the constructor.
            // By default, the gRPC service implementation can resolve other DI services with any lifetime (Singleton, Scoped, or Transient).

            services.AddSingleton<IStockService, StockService>();
            services.AddHostedService<StatusService>();
         
            // gRPC is enabled with the AddGrpc method.
            // services.AddGrpc();

            // Handle error 
            services.AddGrpc(opt =>
            {
                // Exception messages are generally considered sensitive data that shouldn't be revealed to a client.
                // By default, gRPC doesn't send the details of an exception thrown by a gRPC service to the client.
                // Instead, the client receives a generic message indicating an error occurred.
                // Exception message delivery to the client can be overridden (for example, in development or test) with EnableDetailedErrors.
                // Exception messages shouldn't be exposed to the client in production apps.
                opt.EnableDetailedErrors = true;
                
                // opt.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB
                // opt.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
                // opt.ResponseCompressionLevel = CompressionLevel.Optimal; 
                // opt.ResponseCompressionAlgorithm = "gzip";
                   
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapGrpcService<HealthServiceImpl>();
                
                // A gRPC service isn’t based on controllers, rather it uses a service class that’s constructed for every request to process the client call.
                // This service class is called GreeterService in the sample code that the template generates.
                // The MapGrpcService<T> method creates a binding of the URL of the gRPC call to a call handler that gets invoked when processing the request.
                // The handler factory is retrieved and used to create a new instance of the T class for the requested action to take place.
                // This is a key difference between the ASP.NET Core 3.0 implementation of gRPC and the existing C# implementation.
                // Note, however, that it’s still possible that an instance of the service class is resolved as a singleton from the DI container.
                endpoints.MapGrpcService<StockMarketService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }
    }
}