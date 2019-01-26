using MixItUp.API;
using MixItUp.API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    [MixItUpAction("com.mixitup.streamdeckplugin.mixplaystatus")]
    public class MixPlayStatusAction : MixItUpAction
    {
        private StreamDeckConnection connection;

        public string Action { get; private set; }
        public string Context { get; private set; }

        public override async Task LoadAsync(StreamDeckConnection connection, string action, string context, JObject settings)
        {
            this.connection = connection;
            this.Action = action;
            this.Context = context;

            await this.connection.SetTitleAsync("Loading...", this.Context, SDKTarget.HardwareAndSoftware);
        }

        public override Task SaveAsync()
        {
            return Task.FromResult(0);
        }

        public override Task ProcessPropertyInspectorAsync(SendToPluginEvent propertyInspectorEvent)
        {
            return Task.FromResult(0);
        }

        public override Task RunActionAsync()
        {
            return Task.FromResult(0);
        }

        public override Task TitleParametersDidChangeAsync(TitleParametersDidChangeEvent titleParametersDidChangeEvent)
        {
            return Task.FromResult(0);
        }

        public override async Task RunTickAsync()
        {
            try
            {
                MixPlayStatus status = await MixPlay.GetStatusAsync();
                if (status.IsConnected)
                {
                    await this.connection.SetStateAsync(1, this.Context);
                    string title = status.GameName.Replace(' ', '\n');
                    await this.connection.SetTitleAsync(title, this.Context, SDKTarget.HardwareAndSoftware);
                }
                else
                {
                    await this.connection.SetStateAsync(0, this.Context);
                    await this.connection.SetTitleAsync("Not\nConnected", this.Context, SDKTarget.HardwareAndSoftware);
                }
            }
            catch
            {
                await this.connection.SetStateAsync(0, this.Context);
                await this.connection.SetTitleAsync("Not\nConnected", this.Context, SDKTarget.HardwareAndSoftware);
            }
        }
    }
}
