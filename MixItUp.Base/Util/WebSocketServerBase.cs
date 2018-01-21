using Mixer.Base.Model.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class WebSocketServerBase
    {
        public event EventHandler Disconnected;

        private const int bufferSize = 1000000;

        private SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        private UTF8Encoding encoder = new UTF8Encoding();

        private string address;

        private HttpListener httpListener;
        private WebSocket webSocket;
        private CancellationTokenSource webSocketTokenSource = new CancellationTokenSource();

        private bool connectionTestSuccessful;

        public WebSocketServerBase(string address) { this.address = address; }

        public Task<bool> Initialize()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async() =>
            {
                await BackgroundTaskWrapper.RunBackgroundTask(this.webSocketTokenSource, async (tokenSource) =>
                {
                    if (await this.ListenForConnection())
                    {
                        if (ChannelSession.Settings.DiagnosticLogging)
                        {
                            this.Send(new WebSocketPacket() { type = "debug" });
                        }

                        await this.ReceiveInternal();
                        await this.ShutdownWebsocket();
                    }
                    else
                    {
                        this.OnDisconnected();
                        this.webSocketTokenSource.Cancel();
                    }
                });
            }, this.webSocketTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Task.FromResult(true);
        }

        public async Task Send(WebSocketPacket packet)
        {
            try
            {
                string packetJson = JsonConvert.SerializeObject(packet);
                byte[] buffer = this.encoder.GetBytes(packetJson);

                await this.asyncSemaphore.WaitAsync();

                await this.webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally { this.asyncSemaphore.Release(); }
        }

        public async Task Disconnect()
        {
            this.webSocketTokenSource.Cancel();
            this.ShutdownListener();
            await this.ShutdownWebsocket();
        }

        public async Task<bool> TestConnection()
        {
            this.connectionTestSuccessful = false;
            await this.Send(new WebSocketPacket() { type = "test" });
            for (int i = 0; i < 5 && !this.connectionTestSuccessful; i++)
            {
                await Task.Delay(500);
            }
            return this.connectionTestSuccessful;
        }

        protected virtual Task PacketReceived(string packet)
        {
            if (packet != null)
            {
                JObject packetObj = JObject.Parse(packet);
                if (packetObj["type"].ToString().Equals("exception"))
                {
                    Logger.Log("WebSocket Client Exception: " + packetObj["data"].ToString());
                }
                else if (packetObj["type"].ToString().Equals("test"))
                {
                    this.connectionTestSuccessful = true;
                }
            }
            return Task.FromResult(0);
        }

        private async Task<bool> ListenForConnection()
        {
            this.webSocket = null;
            try
            {
                this.httpListener = new HttpListener();
                this.httpListener.Prefixes.Add(this.address);
                this.httpListener.Start();

                var hc = await this.httpListener.GetContextAsync();
                if (hc.Request.IsWebSocketRequest)
                {
                    var wsc = await hc.AcceptWebSocketAsync(null);
                    this.webSocket = wsc.WebSocket;
                    return true;
                }
                else
                {
                    hc.Response.StatusCode = 400;
                    hc.Response.Close();
                    this.httpListener = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            this.ShutdownListener();
            return false;
        }

        private async Task ReceiveInternal()
        {
            await Task.Delay(100);

            byte[] buffer = new byte[WebSocketServerBase.bufferSize];
            while (this.webSocket != null && this.webSocket.State == WebSocketState.Open)
            {
                try
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    WebSocketReceiveResult result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result != null)
                    {
                        if (result.CloseStatus == null || result.CloseStatus != WebSocketCloseStatus.Empty)
                        {
                            try
                            {
                                string jsonBuffer = this.encoder.GetString(buffer);
                                dynamic jsonObject = JsonConvert.DeserializeObject(jsonBuffer);

                                await this.PacketReceived(jsonBuffer);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void ShutdownListener()
        {
            try
            {
                if (this.httpListener != null)
                {
                    this.httpListener.Stop();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            this.httpListener = null;
        }

        private async Task ShutdownWebsocket()
        {
            this.OnDisconnected();
            try
            {
                if (this.webSocket != null && this.webSocket.State == WebSocketState.Open && this.webSocket.CloseStatus != null)
                {
                    await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            this.webSocket = null;
        }

        private void OnDisconnected()
        {
            if (this.Disconnected != null)
            {
                this.Disconnected(this, new EventArgs());
            }
        }
    }
}
