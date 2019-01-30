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

        public override async Task ProcessPropertyInspectorAsync(SendToPluginEvent propertyInspectorEvent)
        {
            switch (propertyInspectorEvent.Payload["property_inspector"].ToString().ToLower())
            {
                case "propertyinspectorconnected":
                    await this.ConnectPropertiesAsync();
                    break;
                case "propertyinspectorwilldisappear":
                    break;
                case "updatesettings":
                    break;
            }
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

        private async Task ConnectPropertiesAsync()
        {
            bool isDeveloperAPIEnabled = true;
            try
            {
                MixPlayStatus _ = await MixPlay.GetStatusAsync();
            }
            catch
            {
                isDeveloperAPIEnabled = false;
            }

            if (isDeveloperAPIEnabled)
            {
                // Blank object to clear the loading
                JObject response = new JObject();
                await this.connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
            }
            else
            {
                // Developer API is not enabled
                JObject response = new JObject
                {
                    ["error"] = JValue.CreateString("developerAPINotEnabled")
                };
                await this.connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
            }
        }
    }
}
