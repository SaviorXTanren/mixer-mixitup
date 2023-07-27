using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.SignalR.Client
{
    public class SignalRConnection
    {
        public string Address { get; private set; }

        public event EventHandler Connected;
        public event EventHandler<Exception> Disconnected;

        private HubConnection connection;

        public SignalRConnection(string address)
        {
            this.Address = address;
            this.connection = new HubConnectionBuilder()
                .AddJsonProtocol()
                .WithUrl(this.Address, opts =>
                {
                    opts.SkipNegotiation = true;
                    opts.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                }).Build();
        }

        public void IncreaseDefaultConnectionLimit() { ServicePointManager.DefaultConnectionLimit = 10; }

        public void Listen(string methodName, Action handler) { this.connection.On(methodName, handler); }
        public void Listen<T1>(string methodName, Action<T1> handler) { this.connection.On<T1>(methodName, handler); }
        public void Listen<T1, T2>(string methodName, Action<T1, T2> handler) { this.connection.On<T1, T2>(methodName, handler); }
        public void Listen<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler) { this.connection.On<T1, T2, T3>(methodName, handler); }
        public void Listen<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler) { this.connection.On<T1, T2, T3, T4>(methodName, handler); }

        public async Task<bool> Connect()
        {
            try
            {
                this.connection.Closed -= Connection_Closed;
                this.connection.Closed += Connection_Closed;

                await this.connection.StartAsync();
                this.Connected?.Invoke(this, new EventArgs());
                return true;
            }
            catch { }
            return false;
        }

        public bool IsConnected() { return this.connection.State == HubConnectionState.Connected; }

        public async Task Send(string methodName) { await this.connection.InvokeAsync(methodName); }
        public async Task Send(string methodName, object item1) { await this.connection.InvokeAsync(methodName, item1); }
        public async Task Send(string methodName, object item1, object item2) { await this.connection.InvokeAsync(methodName, item1, item2); }
        public async Task Send(string methodName, object item1, object item2, object item3) { await this.connection.InvokeAsync(methodName, item1, item2, item3); }

        public async Task Disconnect()
        {
            this.connection.Closed -= Connection_Closed;
            await this.connection.StopAsync();
        }

        private async Task Connection_Closed(Exception ex)
        {
            await this.Disconnect();
            this.Disconnected?.Invoke(this, ex);
        }
    }
}
