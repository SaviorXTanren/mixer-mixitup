using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Util
{
    public abstract class SocketIOService
    {
        protected Socket socket;

        private string connectionURL;
        private string query;

        public SocketIOService(string connectionURL) { this.connectionURL = connectionURL; }

        public SocketIOService(string connectionURL, string query) : this(connectionURL) { this.query = query; }

        public virtual Task Connect()
        {
            this.socket = !string.IsNullOrEmpty(this.query) ? IO.Socket(this.connectionURL, new IO.Options() { QueryString = this.query }) : IO.Socket(this.connectionURL);
            return Task.FromResult(0);
        }

        public virtual Task Disconnect()
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

        protected void SocketReceiveWrapper(string eventString, Action<object> processEvent)
        {
            if (!this.socket.HasListeners(eventString))
            {
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

        protected void SocketEventReceiverWrapper<T>(string eventString, Action<T> processEvent)
        {
            this.SocketReceiveWrapper(eventString, (eventData) =>
            {
                JObject jobj = JObject.Parse(eventData.ToString());
                if (jobj != null)
                {
                    processEvent(jobj.ToObject<T>());
                }
            });
        }

        protected void SocketSendWrapper(string eventString, object data)
        {
            try
            {
                this.socket.Emit(eventString, data);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
