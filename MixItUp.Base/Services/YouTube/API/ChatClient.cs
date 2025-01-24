using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// Handles interaction with Live Stream-based chat services.
    /// </summary>
    public class ChatClient : YouTubeServiceBase, IDisposable
    {
        private int minimumPollTimeMilliseconds;

        private CancellationTokenSource messageBackgroundPollingTokenSource;

        /// <summary>
        /// <see cref="LiveChatMessagesResultModel.NextResultsToken"/> from the previous live chat request.
        /// </summary>
        private string nextResultsToken;

        private LiveBroadcast broadcast;

        /// <summary>
        /// The live broadcast connected to for chat.
        /// </summary>
        public LiveBroadcast Broadcast
        {
            get { return this.broadcast; }
            private set
            {
                this.broadcast = value;
                this.nextResultsToken = null; // also clear the next message token, as it's no longer valid for the new broadcast
            }
        }

        /// <summary>
        /// Invoked when chat messages are received.
        /// </summary>
        public event EventHandler<IEnumerable<LiveChatMessage>> OnMessagesReceived;

        /// <summary>
        /// Creates a new instance of the ChatClient class.
        /// </summary>
        /// <param name="connection">The connection to YouTube</param>
        public ChatClient(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Connects to the chat for the current broadcast.
        /// </summary>
        /// <param name="listenForMessage">Whether to enable message listen polling</param>
        /// <param name="minimumPollTimeMilliseconds">The minimum amount of milliseconds to wait in-between message polls, regardless of the polling interval returned by YouTube</param>
        /// <returns>Whether the connection was successful</returns>
        public async Task<bool> Connect(bool listenForMessage = true, int minimumPollTimeMilliseconds = 0)
        {
            return await this.Connect(null, listenForMessage, minimumPollTimeMilliseconds);
        }

        /// <summary>
        /// Connects to the specified broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast to connect to</param>
        /// <param name="listenForMessage">Whether to enable message listen polling</param>
        /// <param name="minimumPollTimeMilliseconds">The minimum amount of milliseconds to wait in-between message polls, regardless of the polling interval returned by YouTube</param>
        /// <returns>Whether the connection was successful</returns>
        public Task<bool> Connect(LiveBroadcast broadcast, bool listenForMessage = true, int minimumPollTimeMilliseconds = 0)
        {
            this.Broadcast = broadcast;

            if (listenForMessage)
            {
                this.minimumPollTimeMilliseconds = minimumPollTimeMilliseconds;

                this.messageBackgroundPollingTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(this.MessageBackgroundPolling, this.messageBackgroundPollingTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Disconnectes from the current broadcast.
        /// </summary>
        /// <returns>An awaitable Task</returns>
        public Task Disconnect()
        {
            this.Broadcast = null;
            if (this.messageBackgroundPollingTokenSource != null)
            {
                this.messageBackgroundPollingTokenSource.Cancel();
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sends a message to the live chat.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>The resulting message</returns>
        public async Task<LiveChatMessage> SendMessage(string message) { return await this.connection.LiveChat.SendMessage(this.Broadcast, message); }

        /// <summary>
        /// Deletes the specified message from the live chat.
        /// </summary>
        /// <param name="message">The message to delete</param>
        /// <returns>An awaitable Task</returns>
        public async Task DeleteMessage(LiveChatMessage message) { await this.connection.LiveChat.DeleteMessage(message); }

        /// <summary>
        /// Makes the specified user a moderator.
        /// </summary>
        /// <param name="user">The user to mod</param>
        /// <returns>Information about the modded user</returns>
        public async Task<LiveChatModerator> ModUser(Channel user) { return await this.connection.LiveChat.ModUser(this.Broadcast, user); }

        /// <summary>
        /// Removes moderator privileges from the specified user.
        /// </summary>
        /// <param name="moderator">The user to unmode</param>
        /// <returns>An awaitable Task</returns>
        public async Task UnmodUser(LiveChatModerator moderator) { await this.connection.LiveChat.UnmodUser(moderator); }

        /// <summary>
        /// Times out the specified user from the channel.
        /// </summary>
        /// <param name="user">The user to timeout</param>
        /// <param name="duration">The length of the timeout in seconds</param>
        /// <returns>The timeout result</returns>
        public async Task<LiveChatBan> TimeoutUser(Channel user, ulong duration) { return await this.connection.LiveChat.TimeoutUser(this.Broadcast, user, duration); }

        /// <summary>
        /// Bans the specified user from the channel.
        /// </summary>
        /// <param name="user">The user to ban</param>
        /// <returns>The ban result</returns>
        public async Task<LiveChatBan> BanUser(Channel user) { return await this.connection.LiveChat.BanUser(this.Broadcast, user); }

        // TODO: https://stackoverflow.com/questions/55721986/how-to-look-up-youtube-live-chat-ban-id-to-delete-it
        /// <summary>
        /// Unbans the specified user from the channel.
        /// </summary>
        /// <param name="ban">The ban to remove</param>
        /// <returns>An awaitable Task</returns>
        public async Task UnbanUser(LiveChatBan ban) { await this.connection.LiveChat.UnbanUser(ban); }

        private async Task MessageBackgroundPolling()
        {
            while (!this.messageBackgroundPollingTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (this.Broadcast == null)
                    {
                        this.Broadcast = await this.connection.LiveBroadcasts.GetMyActiveBroadcast();
                    }

                    if (this.Broadcast != null)
                    {
                        LiveChatMessagesResultModel result = await this.connection.LiveChat.GetMessages(this.Broadcast, this.nextResultsToken);
                        if (result != null)
                        {
                            if (result.Messages.Count > 0)
                            {
                                this.OnMessagesReceived?.Invoke(this, result.Messages);
                            }

                            this.nextResultsToken = result.NextResultsToken;
                            int pollingInterval = Math.Max((int)result.PollingInterval, this.minimumPollTimeMilliseconds);
                            await Task.Delay(pollingInterval);
                        }
                        else
                        {
                            await Task.Delay(10000);
                        }
                    }
                    else
                    {
                        await Task.Delay(60000);
                    }

                }
                catch (TaskCanceledException) { }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes the resources of the object.
        /// </summary>
        /// <param name="disposing">Whether disposal is taking place</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.messageBackgroundPollingTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the resources of the object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
