using Newtonsoft.Json.Linq;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.StreamDeckPlugin
{
    public class MixItUpPlugin
    {
        private static readonly Dictionary<string, Type> actionList = new Dictionary<string, Type>();

        private StreamDeckConnection connection;
        private ManualResetEvent connectEvent = new ManualResetEvent(false);
        private ManualResetEvent disconnectEvent = new ManualResetEvent(false);
        private bool isMixItUpRunning = false;
        private readonly Dictionary<string, MixItUpAction> actions = new Dictionary<string, MixItUpAction>();

        static MixItUpPlugin()
        {
            Type[] types = typeof(MixItUpPlugin).Assembly.GetTypes();
            foreach (Type type in types)
            {
                object[] attributes = type.GetCustomAttributes(typeof(MixItUpActionAttribute), false);
                if (attributes.Length > 0)
                {
                    MixItUpActionAttribute actionAttribute = attributes[0] as MixItUpActionAttribute;
                    if (actionAttribute != null)
                    {
                        actionList[actionAttribute.ActionName.ToLower()] = type;
                    }
                }
            }
        }

        public void Run(int port, string uuid, string registerEvent)
        {
            this.connection = new StreamDeckConnection(port, uuid, registerEvent);

            this.connection.OnConnected += Connection_OnConnected;
            this.connection.OnDisconnected += Connection_OnDisconnected;
            this.connection.OnApplicationDidLaunch += Connection_OnApplicationDidLaunch;
            this.connection.OnApplicationDidTerminate += Connection_OnApplicationDidTerminate;
            this.connection.OnKeyDown += Connection_OnKeyDown;
            this.connection.OnWillAppear += Connection_OnWillAppear;
            this.connection.OnWillDisappear += Connection_OnWillDisappear;
            this.connection.OnSendToPlugin += Connection_OnSendToPlugin;

            // Start the connection
            connection.Run();

            // Wait for up to 10 seconds to connect, if it fails, the app will exit
            if (connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
            {
                // We connected, loop every second until we disconnect
                while (!disconnectEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                {
                    RunTick();
                }
            }
            else
            {
                Console.WriteLine("Mix It Up Plugin failed to connect to Stream Deck");
            }
        }

        private void Connection_OnSendToPlugin(object sender, StreamDeckEventReceivedEventArgs<SendToPluginEvent> e)
        {
            if (!isMixItUpRunning)
            {
                // Send a message explaining this
                JObject response = new JObject();
                response["error"] = JValue.CreateString("mixItUpIsNotRunning");
                _ = this.connection.SendToPropertyInspectorAsync(e.Event.Action, response, e.Event.Context);
            }
            else
            {
                lock (actions)
                {
                    if (actions.ContainsKey(e.Event.Context.ToLower()))
                    {
                        _ = actions[e.Event.Context.ToLower()].ProcessPropertyInspectorAsync(this.connection, e.Event);
                    }
                }
            }
        }

        private void Connection_OnWillDisappear(object sender, StreamDeckEventReceivedEventArgs<WillDisappearEvent> e)
        {
            lock (actions)
            {
                if (actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    actions[e.Event.Context.ToLower()].Save();
                    actions.Remove(e.Event.Context.ToLower());
                }
            }
        }

        private void Connection_OnWillAppear(object sender, StreamDeckEventReceivedEventArgs<WillAppearEvent> e)
        {
            lock (this.actions)
            {
                if (actionList.ContainsKey(e.Event.Action.ToLower()))
                {
                    MixItUpAction action = Activator.CreateInstance(actionList[e.Event.Action.ToLower()]) as MixItUpAction;
                    action.Load(e.Event.Action, e.Event.Context);
                    this.actions[e.Event.Context.ToLower()] = action;
                }
            }
        }

        private void Connection_OnKeyDown(object sender, StreamDeckEventReceivedEventArgs<KeyDownEvent> e)
        {
            if (!isMixItUpRunning)
            {
                // If Mix It Up isn't running, just don't try
                return;
            }

            lock (actions)
            {
                if (actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    _ = actions[e.Event.Context.ToLower()].RunActionAsync(this.connection);
                }
            }
        }

        private void Connection_OnApplicationDidTerminate(object sender, StreamDeckEventReceivedEventArgs<ApplicationDidTerminateEvent> e)
        {
            if (e.Event.Payload.Application.Equals("mixitup.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                isMixItUpRunning = false;
            }
        }

        private void Connection_OnApplicationDidLaunch(object sender, StreamDeckEventReceivedEventArgs<ApplicationDidLaunchEvent> e)
        {
            if (e.Event.Payload.Application.Equals("mixitup.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                isMixItUpRunning = true;
            }
        }

        private void RunTick()
        {

        }

        private void Connection_OnDisconnected(object sender, EventArgs e)
        {
            this.disconnectEvent.Set();
        }

        private void Connection_OnConnected(object sender, EventArgs e)
        {
            this.connectEvent.Set();
        }
    }
}
