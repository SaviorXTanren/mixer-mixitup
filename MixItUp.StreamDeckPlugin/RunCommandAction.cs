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
    [MixItUpAction("com.mixitup.streamdeckplugin.runcommand")]
    public class RunCommandAction : MixItUpAction
    {
        private StreamDeckConnection connection;

        private class RunCommandSettings
        {
            [JsonProperty]
            public Guid? CommandId { get; set; }

            [JsonProperty]
            public string Arguments { get; set; }

            [JsonProperty]
            public string Title { get; set; }
        }
        private RunCommandSettings actionSettings = new RunCommandSettings();

        public string Action { get; private set; }
        public string Context { get; private set; }

        public override async Task LoadAsync(StreamDeckConnection connection, string action, string context, JObject settings)
        {
            this.connection = connection;
            this.Action = action;
            this.Context = context;

            if (settings != null)
            {
                this.actionSettings = settings.ToObject<RunCommandSettings>();
            }

            await this.connection.SetTitleAsync("Loading...", this.Context, SDKTarget.HardwareAndSoftware);
            await this.RefreshTitleAsync();
        }

        public override async Task SaveAsync()
        {
            await this.connection.SetSettingsAsync(JObject.FromObject(this.actionSettings), this.Context);
        }

        public override async Task ProcessPropertyInspectorAsync(SendToPluginEvent propertyInspectorEvent)
        {
            switch (propertyInspectorEvent.Payload["property_inspector"].ToString().ToLower())
            {
                case "propertyinspectorconnected":
                    await this.ConnectPropertiesAsync();
                    break;
                case "propertyinspectorwilldisappear":
                    await this.SaveAsync();
                    break;
                case "updatesettings":
                    this.actionSettings.CommandId = Guid.Parse(propertyInspectorEvent.Payload["selectedCommandId"].ToString());
                    this.actionSettings.Arguments = propertyInspectorEvent.Payload["arguments"].ToString();
                    await this.SaveAsync();
                    await this.RefreshTitleAsync();
                    break;
            }
        }

        private async Task RefreshTitleAsync()
        {
            if (!string.IsNullOrEmpty(this.actionSettings.Title))
            {
                await this.connection.SetTitleAsync(this.actionSettings.Title, this.Context, SDKTarget.HardwareAndSoftware);
            }
            else if (this.actionSettings.CommandId.HasValue)
            {
                Command command = null;
                try
                {
                    Task<Command> task = Commands.GetCommandAsync(this.actionSettings.CommandId.Value);
                    if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))) == task)
                    {
                        if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                        {
                            command = task.Result;
                        }
                    }
                }
                catch { }

                if (command == null)
                {
                    await this.connection.SetTitleAsync("Command\nNot\nFound", this.Context, SDKTarget.HardwareAndSoftware);
                }
                else
                {
                    string title = command.Name.Replace(' ', '\n');
                    await this.connection.SetTitleAsync(title, this.Context, SDKTarget.HardwareAndSoftware);
                }
            }
            else
            {
                await this.connection.SetTitleAsync("Command\nNot\nSelected", this.Context, SDKTarget.HardwareAndSoftware);
            }
        }

        private async Task ConnectPropertiesAsync()
        {
            Command[] commands = null;

            try
            {
                Task<Command[]> task = Commands.GetAllCommandsAsync();
                if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))) == task)
                {
                    if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                    {
                        commands = task.Result;
                    }
                }
            }
            catch { }

            if (commands == null)
            {
                // Developer API is not enabled
                JObject response = new JObject();
                response["error"] = JValue.CreateString("developerAPINotEnabled");
                await this.connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
            }
            else
            {
                JObject response = new JObject
                {
                    ["commands"] = JArray.FromObject(commands.OrderBy(c => c.Category).ThenBy(c => c.Name)),
                    ["selectedCommandId"] = JValue.CreateString(this.actionSettings.CommandId.ToString()),
                    ["arguments"] = JValue.CreateString(this.actionSettings.Arguments),
                };

                await this.connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
                await this.RefreshTitleAsync();
            }
        }

        public override async Task RunActionAsync()
        {
            if (this.actionSettings.CommandId.HasValue)
            {
                string arguments = this.actionSettings.Arguments ?? string.Empty;
                string[] args = arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                await Commands.RunCommandAsync(this.actionSettings.CommandId.Value, args);
            }
        }

        public override async Task TitleParametersDidChangeAsync(TitleParametersDidChangeEvent titleParametersDidChangeEvent)
        {
            this.actionSettings.Title = titleParametersDidChangeEvent.Payload.Title;
            await this.RefreshTitleAsync();
        }

        public override Task RunTickAsync()
        {
            return Task.FromResult(0);
        }
    }
}
