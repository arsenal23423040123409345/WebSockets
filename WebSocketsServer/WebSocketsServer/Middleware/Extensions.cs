using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebSocketsServer.Services;

namespace WebSocketsServer.Middleware
{
    public static class Extensions
    {
        public static IApplicationBuilder UseCustomWebSocketHandling(this IApplicationBuilder builder)
            => builder.UseMiddleware<WebSocketConnectionHandler>();

        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
            => services.AddSingleton<WebSocketsOperationService>();
    }
}
