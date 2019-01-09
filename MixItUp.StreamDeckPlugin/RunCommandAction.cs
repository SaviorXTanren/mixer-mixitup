using MixItUp.API;
using MixItUp.API.Models;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    [MixItUpAction("com.mixitup.streamdeckplugin.runcommand")]
    public class RunCommandAction : MixItUpAction
    {
        public string Action { get; private set; }
        public string Context { get; private set; }
        public Guid? CommandId { get; set; }

        public override void Load(string action, string context)
        {
            this.Action = action;
            this.Context = context;
        }

        public override void Save()
        {
        }

        public override Task ProcessPropertyInspectorAsync(StreamDeckConnection connection, SendToPluginEvent propertyInspectorEvent)
        {
            switch(propertyInspectorEvent.Payload["property_inspector"].ToString().ToLower())
            {
                case "propertyinspectorconnected":
                    _ = ConnectPropertiesAsync(connection);
                    break;
            }

            return Task.FromResult(0);
        }

        private async Task ConnectPropertiesAsync(StreamDeckConnection connection)
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
                _ = connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
            }
            else
            {
                JObject response = new JObject
                {
                    ["commands"] = JArray.FromObject(commands.OrderBy(c => c.Name))
                };

                _ = connection.SendToPropertyInspectorAsync(this.Action, response, this.Context);
            }
        }

        public override Task RunActionAsync(StreamDeckConnection connection)
        {
            if (this.CommandId.HasValue)
            {
                _ = Commands.RunCommandAsync(this.CommandId.Value);
            }

            return Task.FromResult(0);
        }
    }
}
