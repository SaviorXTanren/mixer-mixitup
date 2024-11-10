using MixItUp.Base.Util;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    /// <summary>
    /// Handles web socket communication for both client &amp; server connections.
    /// </summary>
    public abstract class WebSocketBase
    {
        /// <summary>
        /// The base buffer size for websocket buffers.
        /// </summary>
        protected const int BUFFER_SIZE = 1000000;

        /// <summary>
        /// Invoked when a packet is sent.
        /// </summary>
        public event EventHandler<string> OnSentOccurred = delegate { };
        /// <summary>
        /// Invoked when a text packet is received.
        /// </summary>
        public event EventHandler<string> OnTextReceivedOccurred = delegate { };
        /// <summary>
        /// Invoked when an unexpected disconnection occurs.
        /// </summary>
        public event EventHandler<WebSocketCloseStatus> OnDisconnectOccurred = delegate { };

        /// <summary>
        /// The web socket connection.
        /// </summary>
        protected WebSocket webSocket;

        /// <summary>
        /// Locking semaphore to prevent clashing packet sends.
        /// </summary>
        protected SemaphoreSlim webSocketSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Disconnects the web socket.
        /// </summary>
        /// <param name="closeStatus">Optional status to send to partner web socket as to why the web socket is being closed</param>
        /// <returns>A task for the closing of the web socket</returns>
        public virtual Task Disconnect(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            if (this.webSocket != null)
            {
                try
                {
                    if (GetState() != WebSocketState.Closed)
                    {
                        this.webSocket.CloseAsync(closeStatus, string.Empty, CancellationToken.None).Wait(1);
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            this.webSocket = null;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Sends a text packet to the connected web socket.
        /// </summary>
        /// <param name="packet">The text packet to send</param>
        /// <returns>A task for the sending of the packet</returns>
        public virtual async Task Send(string packet)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(packet);

            await this.webSocketSemaphore.WaitAsync();

            await this.SendInternal(buffer);

            this.webSocketSemaphore.Release();

            this.OnSentOccurred?.Invoke(this, packet);
        }

        /// <summary>
        /// Gets whether the web socket is currently open.
        /// </summary>
        /// <returns>Whether the web socket is currently open</returns>
        public bool IsOpen() { return this.GetState() == WebSocketState.Open; }

        /// <summary>
        /// Gets the current state of the web socket.
        /// </summary>
        /// <returns>The current state of the web socket</returns>
        public WebSocketState GetState()
        {
            return GetState(this.webSocket);
        }

        /// <summary>
        /// Gets the state of the specified web socket.
        /// </summary>
        /// <param name="webSocket">The web socket to examine.</param>
        /// <returns>The current state of the web socket</returns>
        protected static WebSocketState GetState(WebSocket webSocket)
        {
            try
            {
                if (webSocket != null && (webSocket.CloseStatus == null || webSocket.CloseStatus == WebSocketCloseStatus.Empty))
                {
                    return webSocket.State;
                }
            }
            finally { }
            return WebSocketState.Closed;
        }

        /// <summary>
        /// Sets the web socket connection to use.
        /// </summary>
        /// <param name="webSocket">The web socket connection</param>
        protected void SetWebSocket(WebSocket webSocket) { this.webSocket = webSocket; }

        /// <summary>
        /// Sends the packet of data.
        /// </summary>
        /// <param name="buffer">The buffer to send</param>
        /// <returns>An awaitable task</returns>
        protected abstract Task SendInternal(byte[] buffer);

        /// <summary>
        /// Processes the received text packet.
        /// </summary>
        /// <param name="packet">The receive text packet</param>
        /// <returns>An awaitable task</returns>
        protected abstract Task ProcessReceivedPacket(string packet);

        /// <summary>
        /// Handles all receiving &amp; processing of packets.
        /// </summary>
        /// <returns>An awaitable task with the close status of the web socket connection</returns>
        protected virtual async Task<WebSocketCloseStatus> Receive()
        {
            string jsonBuffer = string.Empty;
            byte[] buffer = new byte[WebSocketBase.BUFFER_SIZE];
            ArraySegment<byte> arrayBuffer = new ArraySegment<byte>(buffer);

            WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure;

            try
            {
                while (this.IsOpen())
                {
                    try
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                        WebSocketReceiveResult result = await this.webSocket.ReceiveAsync(arrayBuffer, CancellationToken.None);

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

        /// <summary>
        /// Used to trigger the OnTextReceivedOccurred event from inherited classes.
        /// </summary>
        /// <param name="jsonBuffer">The contents of the text buffer.</param>
        protected void TextReceivedOccurred(string jsonBuffer)
        {
            this.OnTextReceivedOccurred?.Invoke(this, jsonBuffer);
        }

        /// <summary>
        /// Disconnects the web socket connection and fires a disconnection event.
        /// </summary>
        /// <param name="closeStatus">The close status of the web socket connection</param>
        /// <returns>An awaitable task</returns>
        protected async Task DisconnectAndFireEvent(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await this.Disconnect(closeStatus);

            Task.Run(() => { this.OnDisconnectOccurred?.Invoke(this, closeStatus); }).Wait(1);
        }

        /// <summary>
        /// Waits for an successful operation to complete.
        /// </summary>
        /// <param name="valueToCheck">Where the operation was successful</param>
        /// <param name="secondsToWait">The total amount of seconds to wait for success</param>
        /// <returns>An awaitable task</returns>
        protected async Task WaitForSuccess(Func<bool> valueToCheck, int secondsToWait = 15)
        {
            int loops = (secondsToWait * 1000) / 100;
            for (int i = 0; i < loops && !valueToCheck(); i++)
            {
                await Task.Delay(100);
            }
        }
    }
}
