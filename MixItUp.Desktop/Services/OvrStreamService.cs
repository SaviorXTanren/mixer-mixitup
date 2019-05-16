using Mixer.Base.Clients;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ovrstream_client_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MixItUp.OvrStream
{
    public class OvrStreamService : IOvrStreamService
    {
        OvrStreamConnection connection;
        private Uri address;

        public OvrStreamService(string address)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
            {
                this.address = uri;
            }
        }

        public async Task<bool> Connect()
        {
            try
            {
                this.connection = new OvrStreamConnection(this.address);
                await this.connection.ConnectAsync(CancellationToken.None);

                this.connection.OnDisconnected += Connection_OnDisconnected;
                GlobalEvents.ServiceReconnect("OvrStream");

                return true;
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return false;
        }

        public async Task Disconnect()
        {
            if (this.connection != null)
            {
                this.connection.OnDisconnected -= Connection_OnDisconnected;
                await this.connection.DisconnectAsync(CancellationToken.None);
            }
        }

        public Task UpdateVariables(string titleName, Dictionary<string, string> variables)
        {
            return this.connection.UpdateVariablesAsync(titleName, variables, CancellationToken.None);
        }

        public Task HideTitle(string titleName)
        {
            return this.connection.HideTitleAsync(titleName, CancellationToken.None);
        }

        public async Task PlayTitle(string titleName, Dictionary<string, string> variables)
        {
            await this.connection.UpdateVariablesAsync(titleName, variables, CancellationToken.None);
            await this.connection.ShowTitleAsync(titleName, CancellationToken.None);
        }

        public Task<string> DownloadImage(string uri)
        {
            return this.connection.DownloadImageAsync(new Uri(uri), CancellationToken.None);
        }

        private async void Connection_OnDisconnected(object sender, EventArgs e)
        {
            GlobalEvents.ServiceDisconnect("OvrStream");

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.Connect());

            GlobalEvents.ServiceReconnect("OvrStream");
        }
    }
}
