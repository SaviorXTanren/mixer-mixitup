using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// Client for connecting to chat.
    /// 
    /// https://docs.google.com/document/d/e/2PACX-1vQNr5Tn2A9cS241hQLiF8R1bg-3mKEndYHJJBohNllcpYDRbV5nrBaCAvL4R-vKC1x_kEzFYmaisrtP/pub
    /// </summary>
    public class ChatClient : ClientWebSocketBase
    {
        /// <summary>
        /// The connection url for Trovo's chat servers.
        /// </summary>
        public const string TrovoChatConnectionURL = "wss://open-chat.trovo.live/chat";

        /// <summary>
        /// Invoked when a chat message is received.
        /// </summary>
        public event EventHandler<ChatMessageContainerModel> OnChatMessageReceived = delegate { };

        private TrovoConnection connection;

        private CancellationTokenSource backgroundPingCancellationTokenSource;

        private readonly Dictionary<string, ChatPacketModel> replyIDListeners = new Dictionary<string, ChatPacketModel>();

        /// <summary>
        /// Creates an instance of the ChatClient.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        public ChatClient(TrovoConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Connects the client to the specified channel.
        /// </summary>
        /// <param name="token">The connection token to the channel</param>
        /// <returns>Whether the connection was successful</returns>
        public new async Task<bool> Connect(string token)
        {
            Validator.ValidateString(token, "token");
            if (await base.Connect(TrovoChatConnectionURL))
            {
                ChatPacketModel authReply = await this.SendAndListen(new ChatPacketModel("AUTH", new JObject() { { "token", token } }));
                if (authReply != null && string.IsNullOrEmpty(authReply.error))
                {
                    this.backgroundPingCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => this.BackgroundPing(this.backgroundPingCancellationTokenSource.Token), this.backgroundPingCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public async Task Disconnect()
        {
            if (this.backgroundPingCancellationTokenSource != null)
            {
                this.backgroundPingCancellationTokenSource.Cancel();
                this.backgroundPingCancellationTokenSource = null;
            }
            await base.Disconnect();
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable Task</returns>
        public async Task SendMessage(string message)
        {
            await this.connection.Chat.SendMessage(message);
        }

        /// <summary>
        /// Sends a message to the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to send to</param>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable Task</returns>
        public async Task SendMessage(string channelID, string message)
        {
            await this.connection.Chat.SendMessage(channelID, message);
        }

        /// <summary>
        /// Deletes the specified message in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to delete</param>
        /// <param name="messageID">The ID of the message to delete</param>
        /// <param name="userID">The ID of the user who sent the message</param>
        /// <returns>Whether the delete was successful</returns>
        public async Task<bool> DeleteMessage(string channelID, string messageID, string userID)
        {
            return await this.connection.Chat.DeleteMessage(channelID, messageID, userID);
        }

        /// <summary>
        /// Performs an official Trovo command in the specified channel.
        /// </summary>
        /// <param name="channelID">The ID of the channel to perform the command in</param>
        /// <param name="command">The command to perform</param>
        /// <returns>Null if successful, a status message indicating why the command failed to perform</returns>
        public async Task<string> PerformChatCommand(string channelID, string command)
        {
            return await this.connection.Chat.PerformChatCommand(channelID, command);
        }

        /// <summary>
        /// Sends a ping message to the servers.
        /// </summary>
        /// <returns>The response packet</returns>
        public async Task<ChatPacketModel> Ping()
        {
            return await this.SendAndListen(new ChatPacketModel("PING"));
        }

        /// <summary>
        /// Sends the specified packet.
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <returns>An awaitable Task</returns>
        protected async Task Send(ChatPacketModel packet) { await this.Send(JSONSerializerHelper.SerializeToString(packet)); }

        /// <summary>
        /// Sends a packet to the server and listens for a reply.
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <returns>The reply packet</returns>
        protected async Task<ChatPacketModel> SendAndListen(ChatPacketModel packet)
        {
            ChatPacketModel replyPacket = null;
            this.replyIDListeners[packet.nonce] = null;
            await this.Send(packet);

            await this.WaitForSuccess(() =>
            {
                if (this.replyIDListeners.ContainsKey(packet.nonce) && this.replyIDListeners[packet.nonce] != null)
                {
                    replyPacket = this.replyIDListeners[packet.nonce];
                    return true;
                }
                return false;
            }, secondsToWait: 5);

            this.replyIDListeners.Remove(packet.nonce);
            return replyPacket;
        }

        /// <summary>
        /// Processes a received text packet.
        /// </summary>
        /// <param name="packet">The text packet received</param>
        /// <returns>An awaitable Task</returns>
        protected override Task ProcessReceivedPacket(string packet)
        {
            Logger.Log(LogLevel.Debug, "Trovo Chat Packet: " + packet);

            ChatPacketModel response = JSONSerializerHelper.DeserializeFromString<ChatPacketModel>(packet);
            if (response != null && !string.IsNullOrEmpty(response.type))
            {
                switch (response.type)
                {
                    case "RESPONSE":
                        if (this.replyIDListeners.ContainsKey(response.nonce))
                        {
                            this.replyIDListeners[response.nonce] = response;
                        }
                        break;
                    case "CHAT":
                        this.SendSpecificPacket(response, this.OnChatMessageReceived);
                        break;
                }
            }

            return Task.FromResult(0);
        }

        private void SendSpecificPacket<T>(ChatPacketModel packet, EventHandler<T> eventHandler)
        {
            if (packet.data != null)
            {
                eventHandler?.Invoke(this, packet.data.ToObject<T>());
            }
        }

        private async Task BackgroundPing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int delay = 30;
                try
                {
                    ChatPacketModel reply = await this.Ping();
                    if (reply != null && reply.data != null && reply.data.ContainsKey("gap"))
                    {
                        int.TryParse(reply.data["gap"].ToString(), out delay);
                    }
                    await Task.Delay(delay * 1000);
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }
    }
}
