using Mixer.Base.Clients;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MixItUp.OvrStream
{
    public enum MessageTypes : int
    {
        Signal = 1,
        PropertyUpdate = 2,
        Init = 3,
        Idle = 4,
        Debug = 5,
        InvokeMethod = 6,
        ConnectToSignal = 7,
        DisconnectFromSignal = 8,
        SetProperty = 9,
        Response = 10,
    }

    [JsonObject]
    public class InitMessage : OvrStreamPacket
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.Init; } }
    }

    [JsonObject]
    public class IdleMessage : OvrStreamPacket
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.Idle; } }

        public IdleMessage()
        {
            Id = null;
        }
    }

    [JsonObject]
    public class InvokeMethodMessage : OvrStreamPacket
    {
        [JsonProperty("type")]
        public override MessageTypes MessageType { get { return MessageTypes.InvokeMethod; } }

        [JsonProperty("method")]
        public int Method { get; set; }

        [JsonProperty("args")]
        public object[] Arguments { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }
    }

    public abstract class OvrStreamPacket
    {
        private static long idCounter = 0;

        [JsonProperty("type")]
        public abstract MessageTypes MessageType { get; }

        [JsonProperty("id")]
        public long? Id { get; set; } = GetNextId();

        public static long GetNextId()
        {
            return Interlocked.Increment(ref idCounter);
        }
    }

    [JsonObject]
    public class OvrStreamResponse
    {
        [JsonProperty("type")]
        public MessageTypes MessageType { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }

    internal abstract class NewBlueCommand
    {
        public override string ToString()
        {
            XmlDocument doc = new XmlDocument();
            var rootElement = doc.CreateElement("newblue_ext");

            WriteXml(rootElement);

            return rootElement.OuterXml;
        }

        protected abstract void WriteXml(XmlElement parent);
    }

    internal class ReadTitleCommand : NewBlueCommand
    {
        public int Channel { get; set; }

        public string Title { get; set; }

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "readTitle");
            parent.SetAttribute("channel", Channel.ToString());
            parent.SetAttribute("title", Title);
        }
    }

    internal class ScheduleCommand : NewBlueCommand
    {
        public string Action { get; set; }

        public string Id { get; set; }

        public string Queue { get; set; }

        public Variable[] Data { get; set; } = new Variable[0];

        protected override void WriteXml(XmlElement parent)
        {
            parent.SetAttribute("command", "schedule");
            parent.SetAttribute("action", Action);
            parent.SetAttribute("id", Id);
            parent.SetAttribute("queue", Queue);

            var data = parent.OwnerDocument.CreateElement("data");
            parent.AppendChild(data);

            foreach (var variable in Data)
            {
                var variableElement = parent.OwnerDocument.CreateElement("variable");
                data.AppendChild(variableElement);

                variableElement.SetAttribute("name", variable.Name);
                variableElement.SetAttribute("value", variable.Value);
            }
        }
    }

    internal class Variable
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class OvrStreamService : IOvrStreamService
    {
        private OvrStreamWebSocketClient webSocket;
        private string address;

        public OvrStreamService(string address)
        {
            this.address = address;
        }

        public async Task<bool> Connect()
        {
            try
            {
                this.webSocket = new OvrStreamWebSocketClient(this);
                if (await this.webSocket.Connect(this.address))
                {
                    GlobalEvents.ServiceReconnect("OvrStream");

                    this.webSocket.OnDisconnectOccurred += WebSocket_OnDisconnectOccurred;

                    await this.webSocket.Initialize();
                    return true;
                }
            }
            catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            return false;
        }

        public async Task Disconnect()
        {
            if (this.webSocket != null)
            {
                this.webSocket.OnDisconnectOccurred -= WebSocket_OnDisconnectOccurred;
                await this.webSocket.Disconnect();
            }
        }

        public Task UpdateVariables(string titleName, Dictionary<string, string> variables)
        {
            return this.webSocket.UpdateVariablesAsync(titleName, variables);
        }

        public Task HideTitle(string titleName)
        {
            return this.webSocket.HideTitle(titleName);
        }

        public Task PlayTitle(string titleName, Dictionary<string, string> variables)
        {
            return this.webSocket.PlayTitle(titleName, variables);
        }

        private async void WebSocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
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

    public class OvrStreamWebSocketClient : WebSocketClientBase
    {
        private IOvrStreamService service;
        private Dictionary<long, OvrStreamResponse> responses = new Dictionary<long, OvrStreamResponse>();

        private const string SchedulerName = "scheduler";
        private Dictionary<string, int> schedulerMethods = new Dictionary<string, int>();

        public OvrStreamWebSocketClient(IOvrStreamService service)
        {
            this.service = service;
        }

        public async Task Initialize()
        {
            OvrStreamResponse response = await this.Send(new InitMessage());

            foreach (var method in response.Data[SchedulerName]["methods"])
            {
                JArray methodArray = method as JArray;
                string methodName = methodArray[0].Value<string>();
                int key = methodArray[1].Value<int>();
                this.schedulerMethods[methodName] = key;
            }

            await this.Send(new IdleMessage());
        }

        public async Task PlayTitle(string titleName, IReadOnlyDictionary<string, string> variables)
        {
            await UpdateVariablesAsync(titleName, variables);

            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animatein+override+duration",
                Id = titleName,
                Queue = titleName,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() });
        }

        public async Task HideTitle(string titleName)
        {
            ScheduleCommand command = new ScheduleCommand
            {
                Action = "animateout+override",
                Id = titleName,
                Queue = titleName,
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() });
        }

        public async Task UpdateVariablesAsync(string id, IReadOnlyDictionary<string, string> variables)
        {
            ScheduleCommand command = new ScheduleCommand
            {
                Action = "update",
                Id = id,
                Queue = "Alert",
                Data = variables.Select(kvp => new Variable { Name = kvp.Key, Value = kvp.Value }).ToArray(),
            };

            await InvokeMethodAsync("scheduleCommandXml", new object[] { command.ToString() });
        }

        private Task<OvrStreamResponse> InvokeMethodAsync(string method, object[] arguments)
        {
            if (!this.schedulerMethods.ContainsKey(method))
            {
                throw new InvalidOperationException($"Unknown method on scheduler object: {method}");
            }

            var message = new InvokeMethodMessage
            {
                Object = SchedulerName,
                Method = this.schedulerMethods[method],
                Arguments = arguments,
            };

            return this.Send(message);
        }

        private async Task<OvrStreamResponse> Send(OvrStreamPacket packet)
        {
            string json = JsonConvert.SerializeObject(packet);
            await base.Send(json);

            if (packet.Id.HasValue)
            {
                while (!this.responses.ContainsKey(packet.Id.Value))
                {
                    await Task.Delay(50);
                }

                OvrStreamResponse response = this.responses[packet.Id.Value];
                this.responses.Remove(packet.Id.Value);
                return response;
            }

            return null;
        }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            if (!string.IsNullOrEmpty(packetJSON))
            {
                OvrStreamResponse response = JsonConvert.DeserializeObject<OvrStreamResponse>(packetJSON);
                if (response != null)
                {
                    switch (response.MessageType)
                    {
                        case MessageTypes.Response:
                            this.responses[response.Id] = response;
                            break;

                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
