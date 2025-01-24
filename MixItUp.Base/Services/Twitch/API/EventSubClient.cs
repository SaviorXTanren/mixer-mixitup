using MixItUp.Base.Model.Twitch.Clients.EventSub;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.API
{
    /// <summary>
    /// Web Socket client for interacting with EventSub service.
    /// </summary>
    public class EventSubClient : ClientWebSocketBase
    {
        /// <summary>
        /// The default event sub connection url.
        /// </summary>
        public const string EVENT_SUB_CONNECTION_URL = "wss://eventsub.wss.twitch.tv/ws";

        private readonly IReadOnlyDictionary<MessageType, (Type MessageType, MethodInfo MessageHandler)> MessageTypeMap;

        /// <summary>
        /// Invoked when a first connected, and the session ID is used to subscribe to events.
        /// IMPORTANT You have 10 seconds from the time you receive the Welcome message to subscribe to an event. 
        /// If you don’t subscribe within this timeframe, the server closes the connection.
        /// </summary>
        public event EventHandler<WelcomeMessage> OnWelcomeMessageReceived;
        /// <summary>
        /// The keepalive messages indicate that the WebSocket connection is healthy. The server sends this message 
        /// if Twitch doesn’t deliver an event notification within the keepalive_timeout_seconds window specified in the Welcome message.
        /// If your client doesn’t receive an event or keepalive message for longer than keepalive_timeout_seconds, 
        /// you should assume the connection is lost and reconnect to the server and resubscribe to the events.The
        /// keepalive timer is reset with each notification or keepalive message.
        /// </summary>
        public event EventHandler<KeepAliveMessage> OnKeepAliveMessageReceived;
        /// <summary>
        /// A reconnect message is sent if the server has to drop the connection. The message is sent 30 seconds prior to 
        /// dropping the connection.
        /// The message includes a URL in the reconnect_url field that you should immediately use to create a new connection.
        /// The connection will include the same subscriptions that the old connection had.You should not close the old connection
        /// until you receive a Welcome message on the new connection.
        /// The old connection receives events up until you connect to the new URL and receive the welcome message.
        /// NOTE Twitch sends the old connection a close frame with code 4004 if you connect to the new socket but never 
        /// disconnect from the old socket or you don’t connect to the new socket within the specified timeframe.
        /// </summary>
        public event EventHandler<ReconnectMessage> OnReconnectMessageReceived;
        /// <summary>
        /// A notification message is sent when an event that you subscribe to occurs. The message contains the event’s details.
        /// </summary>
        public event EventHandler<NotificationMessage> OnNotificationMessageReceived;
        /// <summary>
        /// A revocation message is sent if Twitch revokes a subscription. The subscription object’s type field identifies the subscription 
        /// that was revoked, and the status field identifies the reason why the subscription was revoked.
        /// </summary>
        public event EventHandler<RevocationMessage> OnRevocationMessageReceived;

        private WebSocket oldSocket = null;

        /// <summary>
        /// Creates a new EventSub client
        /// </summary>
        public EventSubClient()
        {
            IReadOnlyDictionary<Type, MethodInfo> messageHandlers = typeof(EventSubClient)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.Name == "HandleMessage")
                .ToDictionary(e => e.GetParameters().First().ParameterType, e => e);
            MessageTypeMap = new Dictionary<MessageType, (Type, MethodInfo)>
            {
                { MessageType.session_welcome, (typeof(WelcomeMessage), messageHandlers[typeof(WelcomeMessage)]) },
                { MessageType.session_keepalive, (typeof(KeepAliveMessage), messageHandlers[typeof(KeepAliveMessage)]) },
                { MessageType.session_reconnect, (typeof(ReconnectMessage), messageHandlers[typeof(ReconnectMessage)]) },
                { MessageType.notification, (typeof(NotificationMessage), messageHandlers[typeof(NotificationMessage)]) },
                { MessageType.revocation, (typeof(RevocationMessage), messageHandlers[typeof(RevocationMessage)]) },
            };
        }

        /// <summary>
        /// Connects to the default EventSub connection.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task Connect()
        {
            await base.Connect(EventSubClient.EVENT_SUB_CONNECTION_URL);
        }

        /// <inheritdoc />
        protected override Task ProcessReceivedPacket(string packet)
        {
            if (!string.IsNullOrEmpty(packet))
            {
                JObject jsonData = JObject.Parse(packet);

                string messageTypeString = jsonData["metadata"]?["message_type"]?.Value<string>();
                if (Enum.TryParse<MessageType>(messageTypeString, out MessageType messageType) &&
                    MessageTypeMap.TryGetValue(messageType, out var actualMessageInfo))
                {
                    var payload = jsonData.ToObject(actualMessageInfo.MessageType);
                    actualMessageInfo.MessageHandler.Invoke(this, new object[] { payload });
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Override the receive so we can customize the method
        /// </summary>
        /// <returns></returns>
        protected override async Task<WebSocketCloseStatus> Receive()
        {
            return await Receive(this.webSocket);
        }

        private async Task<WebSocketCloseStatus> Receive(WebSocket currentWebSocket)
        {
            string jsonBuffer = string.Empty;
            byte[] buffer = new byte[WebSocketBase.BUFFER_SIZE];
            ArraySegment<byte> arrayBuffer = new ArraySegment<byte>(buffer);

            WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure;

            try
            {
                while (GetState(currentWebSocket) == WebSocketState.Open)
                {
                    try
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                        WebSocketReceiveResult result = await currentWebSocket.ReceiveAsync(arrayBuffer, CancellationToken.None);

                        if (result != null)
                        {
                            if (result.MessageType == WebSocketMessageType.Close || (result.CloseStatus != null && result.CloseStatus.GetValueOrDefault() != WebSocketCloseStatus.Empty))
                            {
                                closeStatus = result.CloseStatus.GetValueOrDefault();
                            }
                            else if (result.MessageType == WebSocketMessageType.Text)
                            {
                                jsonBuffer += Encoding.UTF8.GetString(buffer, 0, result.Count);
                                if (result.EndOfMessage)
                                {
                                    TextReceivedOccurred(jsonBuffer);

                                    await this.ProcessReceivedPacket(jsonBuffer);
                                    jsonBuffer = string.Empty;
                                }
                            }
                            else
                            {
                                Logger.Log("Unsupported packet received");
                            }
                        }
                    }
                    catch (TaskCanceledException) { }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        closeStatus = WebSocketCloseStatus.InternalServerError;
                        jsonBuffer = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                closeStatus = WebSocketCloseStatus.InternalServerError;
            }

            if (closeStatus != WebSocketCloseStatus.NormalClosure)
            {
                await this.DisconnectAndFireEvent(closeStatus);
            }
            else
            {
                await this.Disconnect(closeStatus);
            }

            return closeStatus;
        }

        private async Task HandleMessage(WelcomeMessage message)
        {
            if (this.oldSocket != null)
            {
                await this.oldSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnected", CancellationToken.None);
                this.oldSocket = null;
            }

            this.OnWelcomeMessageReceived?.Invoke(this, message);
        }

        private Task HandleMessage(KeepAliveMessage message)
        {
            this.OnKeepAliveMessageReceived?.Invoke(this, message);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(ReconnectMessage message)
        {
            // Implement Reconnect Message per: https://dev.twitch.tv/docs/eventsub/handling-websocket-events/#reconnect-message
            // A reconnect message is sent if the server has to drop the connection. The message is sent 30 seconds prior to dropping the connection.
            // The message includes a URL in the reconnect_url field that you should immediately use to create a new connection.
            // The connection will include the same subscriptions that the old connection had. You should not close the old connection until you receive a Welcome message on the new connection.

            // Save the old socket for later disconnect
            this.oldSocket = this.webSocket;

            // Create a new socket and start listening
            ClientWebSocket newSocket = this.CreateWebSocket();
            await newSocket.ConnectAsync(new Uri(message.Payload.Session.ReconnectUrl), CancellationToken.None);
            this.SetWebSocket(this.webSocket);
            this.Receive(newSocket).Wait(1);

            // Trigger message
            this.OnReconnectMessageReceived?.Invoke(this, message);
        }

        private Task HandleMessage(NotificationMessage message)
        {
            this.OnNotificationMessageReceived?.Invoke(this, message);
            return Task.CompletedTask;
        }

        private Task HandleMessage(RevocationMessage message)
        {
            this.OnRevocationMessageReceived?.Invoke(this, message);
            return Task.CompletedTask;
        }
    }
}
