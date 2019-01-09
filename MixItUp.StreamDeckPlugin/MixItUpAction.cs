using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;

namespace MixItUp.StreamDeckPlugin
{
    public abstract class MixItUpAction
    {
        public abstract void Load(string action, string context);
        public abstract void Save();
        public abstract Task RunActionAsync(StreamDeckConnection connection);
        public abstract Task ProcessPropertyInspectorAsync(StreamDeckConnection connection, SendToPluginEvent propertyInspectorEvent);
    }
}
