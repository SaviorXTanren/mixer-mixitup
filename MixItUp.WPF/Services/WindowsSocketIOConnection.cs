using H.Socket.IO;
using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsSocketIOConnection : ISocketIOConnection
    {
        public event EventHandler OnDisconnected = delegate { };

        protected SocketIoClient socket = new SocketIoClient();

        private string connectionURL;

        public WindowsSocketIOConnection() { }

        public async Task Connect(string connectionURL)
        {
            this.connectionURL = connectionURL;
            await this.socket.ConnectAsync(new Uri(this.connectionURL));

            this.socket.Disconnected -= Socket_Disconnected;
            this.socket.Disconnected += Socket_Disconnected;
        }

        public Task Disconnect()
        {
            try
            {
                if (this.socket != null)
                {
                    this.socket.Disconnected -= Socket_Disconnected;
                    this.socket.DisconnectAsync();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return Task.FromResult(0);
        }

        public void Listen(string eventString, Action<object> processEvent)
        {
            try
            {
                this.socket.Off(eventString);
                this.socket.On(eventString, (eventData) =>
                {
                    try
                    {
                        processEvent(eventData);
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void Listen<T>(string eventString, Action<T> processEvent)
        {
            this.Listen(eventString, (eventData) =>
            {
                JObject jobj = JObject.Parse(eventData.ToString());
                if (jobj != null)
                {
                    processEvent(jobj.ToObject<T>());
                }
            });
        }

        public void Send(string eventString, object data)
        {
            try
            {
                this.socket.Emit(eventString, data);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void Socket_Disconnected(object sender, H.WebSockets.Args.WebSocketCloseEventArgs e)
        {
            this.socket.Disconnected -= Socket_Disconnected;
            this.OnDisconnected(this, new EventArgs());
        }
    }
}
