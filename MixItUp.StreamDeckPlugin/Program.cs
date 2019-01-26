using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    class Program
    {
        // StreamDeck launches the plugin with these details
        // -port [number] -pluginUUID [GUID] -registerEvent [string?] -info [json]
        static void Main(string[] args)
        {
            // This makes debugging the plug-in much easer.
            // Uncomment this line, launch stream deck, add a button, attach debugger
            // while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            ParseArgs(args, out int? port, out string uuid, out string registerEvent, out JObject info);

            if (!port.HasValue || string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(registerEvent) || (info == null))
            {
                // Failed to get expected arguments
                Console.WriteLine("Missing arguments.  Expected: -port [number] -pluginUUID [GUID] -registerEvent [string?] -info [json]");
                return;
            }

            MixItUpPlugin plugin = new MixItUpPlugin();
            plugin.RunAsync(port.Value, uuid, registerEvent).Wait();
        }

        private static void ParseArgs(string[] args, out int? port, out string uuid, out string registerEvent, out JObject info)
        {
            port = null;
            uuid = null;
            registerEvent = null;
            info = null;

            if ((args.Length % 2) != 0)
            {
                // Expect an even # of args
            }

            for (int count = 0; count < args.Length; count += 2)
            {
                switch(args[count].ToLower())
                {
                    case "-port":
                        int portValue;
                        if (int.TryParse(args[count + 1], out portValue))
                        {
                            port = portValue;
                        }
                        break;
                    case "-pluginuuid":
                        uuid = args[count + 1];
                        break;
                    case "-registerevent":
                        registerEvent = args[count + 1];
                        break;
                    case "-info":
                        info = JObject.Parse(args[count + 1]);
                        break;
                }
            }
        }
    }
}
