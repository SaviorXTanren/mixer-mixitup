using H.Socket.IO;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsSocketIOConnection : ISocketIOConnection
    {
        public event EventHandler OnConnected = delegate { };
        public event EventHandler OnDisconnected = delegate { };

        protected SocketIoClient socket = new SocketIoClient();

        private string connectionURL;

        public WindowsSocketIOConnection() { }

        public async Task Connect(string connectionURL)
        {
            this.connectionURL = connectionURL;

            this.DisconnectEvents();

            if (ChannelSession.IsDebug())
            {
                this.socket.HandledEventReceived += Socket_HandledEventReceived;
                this.socket.UnhandledEventReceived += Socket_UnhandledEventReceived;
            }
            this.socket.Connected += Socket_Connected;
            this.socket.Disconnected += Socket_Disconnected;
            this.socket.ErrorReceived += Socket_ErrorReceived;
            this.socket.ExceptionOccurred += Socket_ExceptionOccurred;

            await this.socket.ConnectAsync(new Uri(this.connectionURL));
        }

        public Task Disconnect()
        {
            try
            {
                if (this.socket != null)
                {
                    this.DisconnectEvents();
                    this.socket.DisconnectAsync();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }

            return Task.CompletedTask;
        }

        public void Listen(string eventString, Action processEvent)
        {
            this.Remove(eventString);
            try
            {
                this.socket.On(eventString, () =>
                {
                    try
                    {
                        processEvent();
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                });
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void Listen(string eventString, Action<object> processEvent)
        {
            this.Remove(eventString);
            try
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
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void Remove(string eventString)
        {
            try
            {
                this.socket.Off(eventString);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void Send(string eventString, object data)
        {
            try
            {
                this.socket.Emit(eventString, data);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void Socket_Connected(object sender, H.Socket.IO.EventsArgs.SocketIoEventEventArgs e)
        {
            this.OnConnected(this, new EventArgs());
        }

        private void Socket_Disconnected(object sender, H.WebSockets.Args.WebSocketCloseEventArgs e)
        {
            this.DisconnectEvents();
            this.OnDisconnected(this, new EventArgs());
        }

        private void Socket_ErrorReceived(object sender, H.Socket.IO.EventsArgs.SocketIoErrorEventArgs e)
        {
            Logger.Log(LogLevel.Error, "Socket Error Received: " + e.Value);
        }

        private void Socket_ExceptionOccurred(object sender, H.WebSockets.Utilities.DataEventArgs<Exception> e)
        {
            Logger.Log(LogLevel.Error, "Socket Exception Received: " + e.Value);
        }

        private void Socket_HandledEventReceived(object sender, H.Socket.IO.EventsArgs.SocketIoEventEventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Socket Handled Data Received: " + e.Value);
        }

        private void Socket_UnhandledEventReceived(object sender, H.Socket.IO.EventsArgs.SocketIoEventEventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Socket Unhandled Data Received: " + e.Value);
        }

        private void DisconnectEvents()
        {
            this.socket.HandledEventReceived -= Socket_HandledEventReceived;
            this.socket.UnhandledEventReceived -= Socket_UnhandledEventReceived;
            this.socket.Connected -= Socket_Connected;
            this.socket.Disconnected -= Socket_Disconnected;
            this.socket.ErrorReceived -= Socket_ErrorReceived;
            this.socket.ExceptionOccurred -= Socket_ExceptionOccurred;
        }
    }
}
