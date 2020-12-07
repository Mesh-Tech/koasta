using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Koasta.Shared.Database;
using Microsoft.Extensions.Logging;
using Koasta.Shared.Configuration;
using Koasta.Service.EventService.Consumers;

namespace Koasta.Service.EventService.Middleware
{
    public class WebsocketMiddleware
    {
        private readonly RequestDelegate next;
        private readonly EmployeeSessionRepository employeeSessions;
        private readonly ILogger logger;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly ILoggerFactory loggerFactory;
        private readonly ISettings settings;
        private readonly Shared.Types.Constants constants;

        public WebsocketMiddleware(RequestDelegate next, EmployeeSessionRepository employeeSessions, ILoggerFactory loggerFactory, ISettings settings, Shared.Types.Constants constants)
        {
            this.constants = constants;
            this.settings = settings;
            this.loggerFactory = loggerFactory;
            this.next = next;
            this.employeeSessions = employeeSessions;
            this.logger = loggerFactory.CreateLogger(nameof(WebsocketMiddleware));
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/event/live", StringComparison.OrdinalIgnoreCase))
            {
                await this.next(context).ConfigureAwait(false);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            if (!context.Request.Query.ContainsKey("tk"))
            {
                context.Response.StatusCode = 401;
                context.Response.Body = Stream.Null;
                return;
            }

            var values = context.Request.Query["tk"];
            if (values.Count == 0)
            {
                context.Response.StatusCode = 401;
                context.Response.Body = Stream.Null;
                return;
            }

            var authToken = values[0];
            var session = await employeeSessions.FetchEmployeeSessionAndEmployeeFromToken(authToken)
                .Ensure(s => s.HasValue, "Employee was found")
                .OnSuccess(s => s.Value)
                .ConfigureAwait(false);

            if (session.IsFailure)
            {
                context.Response.StatusCode = 401;
                context.Response.Body = Stream.Null;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

            using var consumer = new DelegatingOrderEventConsumer(settings, constants, loggerFactory, session.Value.VenueId);
            consumer.OnNewEvent += async (_, data) =>
            {
                logger.LogDebug("Received order update message for web consumers. Transmitting to connected sockets.");
                await Broadcast(webSocket, Encoding.UTF8.GetBytes(data)).ConfigureAwait(false);
            };
            consumer.StartConsuming();

            await Receive(webSocket, async (_, buffer) =>
            {
                WebsocketStatusMessage message = null;
                try
                {
                    var rawMessage = Encoding.UTF8.GetString(buffer);
                    message = JsonConvert.DeserializeObject<WebsocketStatusMessage>(rawMessage);
                }
                catch (Exception)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bad Request", CancellationToken.None).ConfigureAwait(false);
                    return;
                }

                if (message == null)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bad Request", CancellationToken.None).ConfigureAwait(false);
                    return;
                }

                var currentSession = await employeeSessions.FetchEmployeeSessionAndEmployeeFromToken(message.AuthToken)
                    .Ensure(s => s.HasValue, "Employee was found")
                    .OnSuccess(s => s.Value)
                    .ConfigureAwait(false);

                if (currentSession.IsFailure)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Unauthorised", CancellationToken.None).ConfigureAwait(false);
                }

                if (currentSession.Value.VenueId != session.Value.VenueId)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Unauthorised", CancellationToken.None).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                       cancellationToken: CancellationToken.None).ConfigureAwait(false);

                handleMessage(result, buffer);
            }
        }

        private async Task Broadcast(WebSocket webSocket, byte[] data)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
