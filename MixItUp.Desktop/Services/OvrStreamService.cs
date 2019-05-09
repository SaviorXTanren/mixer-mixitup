using Mixer.Base.Clients;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        private static int idCounter = 0;

        [JsonProperty("type")]
        public abstract MessageTypes MessageType { get; }

        [JsonProperty("id")]
        public int Id { get; set; } = GetNextId();

        public static int GetNextId()
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

        public Task ShowTitle(string titleName, Dictionary<string, string> variables)
        {
            return this.webSocket.ShowTitle(titleName, variables);
        }

        public Task HideTitle(string titleName, Dictionary<string, string> variables)
        {
            return this.webSocket.HideTitle(titleName, variables);
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
        private Dictionary<int, OvrStreamResponse> responses = new Dictionary<int, OvrStreamResponse>();

        private const string SchedulerName = "scheduler";
        private Dictionary<string, int> schedulerMethods = new Dictionary<string, int>();

        public OvrStreamWebSocketClient(IOvrStreamService service)
        {
            this.service = service;
        }

        public async Task Initialize()
        {
            OvrStreamResponse response = await this.Send(new InitMessage(), true);

            foreach (var method in response.Data[SchedulerName]["methods"])
            {
                JArray methodArray = method as JArray;
                string methodName = methodArray[0].Value<string>();
                int key = methodArray[1].Value<int>();
                this.schedulerMethods[methodName] = key;
            }
        }

        public Task ShowTitle(string titleName, Dictionary<string, string> variables)
        {
            return UpdateAndRun("animatein", titleName, variables);
        }

        public Task HideTitle(string titleName, Dictionary<string, string> variables)
        {
            return UpdateAndRun("animateout", titleName, variables);
        }

        public Task PlayTitle(string titleName, Dictionary<string, string> variables)
        {
            return UpdateAndRun("alert", titleName, variables);
        }

        private async Task<string> GetTitleId(string titleName)
        {
            XmlDocument commandXml = new XmlDocument();
            commandXml.LoadXml("<newblue_ext command='readTitle' channel='-1' title='' />");
            commandXml.DocumentElement.SetAttribute("title", titleName);
            var message = new InvokeMethodMessage
            {
                Object = SchedulerName,
                Method = schedulerMethods["scheduleCommandXml"],
                Arguments = new object[]
                {
                    commandXml.OuterXml,
                }
            };

            var resp = await this.Send(message, true);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(resp.Data.Value<string>());
            var node = doc.SelectSingleNode("//title");
            return node?.Attributes["id"].Value;
        }

        private async Task UpdateAndRun(string action, string titleName, Dictionary<string, string> variables)
        {
            string titleId = await GetTitleId(titleName);

            XmlDocument commandXml = new XmlDocument();
            commandXml.LoadXml("<newblue_ext action='' command='schedule' id='' queue=''><data></data></newblue_ext>");
            commandXml.DocumentElement.SetAttribute("action", $"update+{action}");
            commandXml.DocumentElement.SetAttribute("id", titleId);
            commandXml.DocumentElement.SetAttribute("queue", titleId);

            XmlElement dataNode = commandXml.SelectSingleNode("//data") as XmlElement;
            foreach (var kvp in variables)
            {
                XmlElement variable = commandXml.CreateElement("variable");
                variable.SetAttribute("name", kvp.Key);
                variable.SetAttribute("value", kvp.Value);
                dataNode.AppendChild(variable);
            }
            var message = new InvokeMethodMessage
            {
                Object = SchedulerName,
                Method = schedulerMethods["scheduleCommandXml"],
                Arguments = new object[]
                {
                    commandXml.OuterXml,
                }
            };

            await this.Send(message, false);
        }

        private async Task<OvrStreamResponse> Send(OvrStreamPacket packet, bool waitForResponse)
        {
            if (waitForResponse)
            {
                responses[packet.Id] = null;
            }

            string json = JsonConvert.SerializeObject(packet);
            await base.Send(json);

            do
            {
                await Task.Delay(250);
            }
            while (responses.ContainsKey(packet.Id) && responses[packet.Id] == null);

            OvrStreamResponse response = null;
            if (responses.ContainsKey(packet.Id))
            {
                response = responses[packet.Id];
                responses.Remove(packet.Id);
            }

            return response;
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
                            if (this.responses.ContainsKey(response.Id))
                            {
                                this.responses[response.Id] = response;
                            }
                            break;

                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
