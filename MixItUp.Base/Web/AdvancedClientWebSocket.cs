using MixItUp.Base.Util;
using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    public class AdvancedClientWebSocket
    {
        /// <summary>
        /// The base buffer size for websocket buffers.
        /// </summary>
        protected const int BUFFER_SIZE = 1000000;

        /// <summary>
        /// Invoked when a packet is sent.
        /// </summary>
        public event EventHandler<string> PacketSent = delegate { };
        /// <summary>
        /// Invoked when a text packet is received.
        /// </summary>
        public event EventHandler<string> PacketReceived = delegate { };
        /// <summary>
        /// Invoked when an unexpected disconnection occurs.
        /// </summary>
        public event EventHandler<WebSocketCloseStatus> Disconnected = delegate { };

        /// <summary>
        /// Locking semaphore to prevent clashing packet sends.
        /// </summary>
        protected SemaphoreSlim webSocketSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The web socket connection.
        /// </summary>
        protected ClientWebSocket webSocket;

        /// <summary>
        /// Connects the web socket to the server.
        /// </summary>
        /// <param name="endpoint">The endpoint to connect to</param>
        /// <returns>Whether the connection was successful</returns>
        public virtual async Task<bool> Connect(string endpoint, CancellationToken cancellationToken)
        {
            try
            {
                this.webSocket = new ClientWebSocket();

                await this.webSocket.ConnectAsync(new Uri(endpoint), cancellationToken);

                await Task.Delay(1000);

                this.Receive().Wait(1);

                return IsOpen();
            }
            catch (Exception ex)
            {
                await this.Disconnect();
                if (ex is WebSocketException && ex.InnerException is WebException)
                {
                    WebException webException = (WebException)ex.InnerException;
                    if (webException.Response != null && webException.Response is HttpWebResponse)
                    {
                        HttpWebResponse response = (HttpWebResponse)webException.Response;
                        StreamReader reader = new StreamReader(response.GetResponseStream());
                        string responseString = reader.ReadToEnd();
                        throw new WebSocketException(string.Format("{0} - {1} - {2}", response.StatusCode, response.StatusDescription, responseString), ex);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Disconnects the web socket.
        /// </summary>
        /// <param name="closeStatus">Optional status to send to partner web socket as to why the web socket is being closed</param>
        /// <returns>A task for the closing of the web socket</returns>
        public Task Disconnect(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
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
        /// Sends a JSON-serializable packet to the connected web socket.
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <returns>A task for the sending of the packet</returns>
        public async Task Send(object packet)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(packet));
        }

        /// <summary>
        /// Sends a text packet to the connected web socket.
        /// </summary>
        /// <param name="packet">The text packet to send</param>
        /// <returns>A task for the sending of the packet</returns>
        public async Task Send(string packet)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(packet);

            await this.webSocketSemaphore.WaitAsync();

            if (this.IsOpen())
            {
                await this.webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            this.webSocketSemaphore.Release();

            this.PacketSent?.Invoke(this, packet);
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
            try
            {
                if (this.webSocket != null && (this.webSocket.CloseStatus == null || this.webSocket.CloseStatus == WebSocketCloseStatus.Empty))
                {
                    return this.webSocket.State;
                }
            }
            finally { }
            return WebSocketState.Closed;
        }

        /// <summary>
        /// Handles all receiving &amp; processing of packets.
        /// </summary>
        /// <returns>An awaitable task with the close status of the web socket connection</returns>
        protected virtual async Task<WebSocketCloseStatus> Receive()
        {
            string jsonBuffer = string.Empty;
            byte[] buffer = new byte[AdvancedClientWebSocket.BUFFER_SIZE];
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
                                    this.PacketReceived?.Invoke(this, jsonBuffer);
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

            await this.Disconnect(closeStatus);
            if (closeStatus != WebSocketCloseStatus.NormalClosure)
            {
                Task.Run(() => { this.Disconnected?.Invoke(this, closeStatus); }).Wait(1);
            }

            return closeStatus;
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
