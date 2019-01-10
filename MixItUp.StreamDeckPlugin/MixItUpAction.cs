using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;

namespace MixItUp.StreamDeckPlugin
{
    public abstract class MixItUpAction
    {
        public abstract Task LoadAsync(StreamDeckConnection connection, string action, string context, JObject settings);
        public abstract Task SaveAsync();
        public abstract Task RunActionAsync();
        public abstract Task ProcessPropertyInspectorAsync(SendToPluginEvent propertyInspectorEvent);
    }
}
