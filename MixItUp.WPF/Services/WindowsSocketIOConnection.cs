using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsSocketIOConnection : ISocketIOConnection
    {
        protected Socket socket;

        private string connectionURL;
        private string query;

        public WindowsSocketIOConnection() { }

        public Task Connect(string connectionURL)
        {
            this.connectionURL = connectionURL;
            this.socket = !string.IsNullOrEmpty(this.query) ? IO.Socket(this.connectionURL, new IO.Options() { QueryString = this.query }) : IO.Socket(this.connectionURL);
            return Task.FromResult(0);
        }

        public async Task Connect(string connectionURL, string query)
        {
            this.query = query;
            await this.Connect(connectionURL);
        }

        public Task Disconnect()
        {
            try
            {
                if (this.socket != null)
                {
                    this.socket.Close();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            this.socket = null;
            return Task.FromResult(0);
        }

        public void Listen(string eventString, Action<object> processEvent)
        {
            try
            {
                if (!this.socket.HasListeners(eventString))
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
    }
}
