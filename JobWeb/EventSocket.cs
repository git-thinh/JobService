using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace JobWeb
{
    using WebSocketSendAsync = Func<ArraySegment<byte>, int,/*message type*/ bool, /*end of message*/ CancellationToken, Task>;
    public class EventSocket
    {
        static ConcurrentDictionary<string, WebSocketSendAsync> clients = new ConcurrentDictionary<string, WebSocketSendAsync>() { };

        public static void Send(string text)
        {
            foreach (var connection in clients)
            {
                var buf = new ArraySegment<byte>(System.Text.ASCIIEncoding.ASCII.GetBytes(text));
                try
                {
                    SendAsync(connection.Value, buf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    WebSocketSendAsync o;
                    clients.TryRemove(connection.Key, out o);
                }
            }
        }

        public static void Register(object client)
        {
            clients.TryAdd(Guid.NewGuid().ToString(), (WebSocketSendAsync)client);
        }

        static int EnumToOpCode(WebSocketMessageType webSocketMessageType)
        {
            switch (webSocketMessageType)
            {
                case WebSocketMessageType.Text:
                    return 0x1;
                case WebSocketMessageType.Binary:
                    return 0x2;
                case WebSocketMessageType.Close:
                    return 0x8;
                default:
                    throw new ArgumentOutOfRangeException("webSocketMessageType", webSocketMessageType, String.Empty);
            }
        }

        static Task SendAsync(WebSocketSendAsync _sendAsync, ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            return _sendAsync(buffer, EnumToOpCode(messageType), endOfMessage, cancellationToken);
        }
    }
}