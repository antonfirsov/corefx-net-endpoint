﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NetCoreServer
{
    public class EchoWebSocketHeadersHandler
    {
        private const int MaxBufferSize = 1024;

        public static async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Not a websocket request");

                    return;
                }

                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
                await ProcessWebSocketRequest(socket, context.Request.Headers);

            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.SetStatusDescription(ex.Message);
            }
        }

        private static async Task ProcessWebSocketRequest(WebSocket socket, IHeaderDictionary headers)
        {
            var receiveBuffer = new byte[MaxBufferSize];

            // Reflect all headers and cookies
            var sb = new StringBuilder();
            sb.AppendLine("Headers:");

            foreach (KeyValuePair<string, StringValues> pair in headers)
            {
                sb.Append(pair.Key);
                sb.Append(":");
                sb.AppendLine(pair.Value.ToString());
            }

            byte[] sendBuffer = Encoding.UTF8.GetBytes(sb.ToString());
            await socket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, new CancellationToken());

            // Stay in loop while websocket is open
            while (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseSent)
            {
                var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    if (receiveResult.CloseStatus == WebSocketCloseStatus.Empty)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
                    }
                    else
                    {
                        await socket.CloseAsync(
                            receiveResult.CloseStatus.GetValueOrDefault(),
                            receiveResult.CloseStatusDescription,
                            CancellationToken.None);
                    }

                    continue;
                }
            }
        }
    }
}