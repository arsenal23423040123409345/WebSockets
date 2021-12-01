using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebSocketsServer.Middleware;

namespace WebSocketsServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketManager();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            app.UseCustomWebSocketHandling();

            app.Run(async context =>
            {
                Console.WriteLine("3rd request delegate");

                await context.Response.WriteAsJsonAsync("3rd request delegate");
            });
        }
    }
}
