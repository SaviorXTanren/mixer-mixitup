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
        private SemaphoreSlim actionsLock = new SemaphoreSlim(1);

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

        public async Task RunAsync(int port, string uuid, string registerEvent)
        {
            try
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
                this.connection.OnTitleParametersDidChange += Connection_OnTitleParametersDidChange;

                // Start the connection
                connection.Run();

                // Wait for up to 10 seconds to connect, if it fails, the app will exit
                if (this.connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    // We connected, loop every second until we disconnect
                    while (!this.disconnectEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                    {
                        await RunTickAsync();
                    }
                }
                else
                {
                    Console.WriteLine("Mix It Up Plugin failed to connect to Stream Deck");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mix It Up Plugin crashed: {ex.ToString()}");
            }
        }

        private async void Connection_OnTitleParametersDidChange(object sender, StreamDeckEventReceivedEventArgs<TitleParametersDidChangeEvent> e)
        {
            await this.actionsLock.WaitAsync();
            try
            {
                if (this.actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    await this.actions[e.Event.Context.ToLower()].TitleParametersDidChangeAsync(e.Event);
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
        }

        private async void Connection_OnSendToPlugin(object sender, StreamDeckEventReceivedEventArgs<SendToPluginEvent> e)
        {
            if (!this.isMixItUpRunning)
            {
                // Send a message explaining this
                JObject response = new JObject
                {
                    ["error"] = JValue.CreateString("mixItUpIsNotRunning")
                };
                await this.connection.SendToPropertyInspectorAsync(e.Event.Action, response, e.Event.Context);
                return;
            }

            await this.actionsLock.WaitAsync();
            try
            {
                if (this.actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    await this.actions[e.Event.Context.ToLower()].ProcessPropertyInspectorAsync(e.Event);
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
        }

        private async void Connection_OnWillDisappear(object sender, StreamDeckEventReceivedEventArgs<WillDisappearEvent> e)
        {
            await this.actionsLock.WaitAsync();
            try
            {
                if (this.actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    await this.actions[e.Event.Context.ToLower()].SaveAsync();
                    this.actions.Remove(e.Event.Context.ToLower());
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
        }

        private async void Connection_OnWillAppear(object sender, StreamDeckEventReceivedEventArgs<WillAppearEvent> e)
        {
            await this.actionsLock.WaitAsync();
            try
            {
                if (actionList.ContainsKey(e.Event.Action.ToLower()))
                {
                    MixItUpAction action = Activator.CreateInstance(actionList[e.Event.Action.ToLower()]) as MixItUpAction;
                    await action.LoadAsync(this.connection, e.Event.Action, e.Event.Context, e.Event.Payload.Settings);
                    this.actions[e.Event.Context.ToLower()] = action;
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
        }

        private async void Connection_OnKeyDown(object sender, StreamDeckEventReceivedEventArgs<KeyDownEvent> e)
        {
            if (!this.isMixItUpRunning)
            {
                // If Mix It Up isn't running, just don't try
                return;
            }

            await this.actionsLock.WaitAsync();
            try
            {
                if (this.actions.ContainsKey(e.Event.Context.ToLower()))
                {
                    await this.actions[e.Event.Context.ToLower()].RunActionAsync();
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
        }

        private void Connection_OnApplicationDidTerminate(object sender, StreamDeckEventReceivedEventArgs<ApplicationDidTerminateEvent> e)
        {
            if (e.Event.Payload.Application.Equals("mixitup.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                this.isMixItUpRunning = false;
            }
        }

        private void Connection_OnApplicationDidLaunch(object sender, StreamDeckEventReceivedEventArgs<ApplicationDidLaunchEvent> e)
        {
            if (e.Event.Payload.Application.Equals("mixitup.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                this.isMixItUpRunning = true;
            }
        }

        private async Task RunTickAsync()
        {
            // If the plugin needs to do anything, this runs once per second.
            // It could be used to ping to see if the app has arrived, etc.
            await this.actionsLock.WaitAsync();
            try
            {
                foreach (MixItUpAction action in this.actions.Values)
                {
                    await action.RunTickAsync();
                }
            }
            finally
            {
                this.actionsLock.Release();
            }
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
