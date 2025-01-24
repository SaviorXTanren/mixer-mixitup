using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Web
{
    public enum QtWebSocketPacketType
    {
        signal = 1,
        propertyUpdate = 2,
        init = 3,
        idle = 4,
        debug = 5,
        invokeMethod = 6,
        connectToSignal = 7,
        disconnectFromSignal = 8,
        setProperty = 9,
        response = 10,
    };

    public class QtWebSocketPacket
    {
        public int id { get; set; } = -1;
        public QtWebSocketPacketType type { get; set; }
        [JsonProperty("object")]
        public string obj { get; set; }
        public string method { get; set; }
        public JArray args { get; set; } = new JArray();
        public JToken data { get; set; }
    }

    public class QtClientWebSocket : AdvancedClientWebSocket
    {
        public event EventHandler<QtWebSocketPacket> QtPacketReceived = delegate { };

        public int PacketID { get; private set; }

        private Dictionary<int, QtWebSocketPacket> receivedPackets = new Dictionary<int, QtWebSocketPacket>();

        public QtClientWebSocket()
            : base()
        {
            this.PacketReceived += QtClientWebSocket_PacketReceived;
        }

        public override async Task<bool> Connect(string endpoint, CancellationToken cancellationToken)
        {
            this.PacketID = 0;
            this.receivedPackets.Clear();
            return await base.Connect(endpoint, cancellationToken);
        }

        public async Task<QtWebSocketPacket> Init()
        {
            return await this.SendAndReceive(new QtWebSocketPacket()
            {
                type = QtWebSocketPacketType.init
            });
        }

        public async Task InvokeMethod(string obj, string method, List<object> arguments)
        {
            await this.Send(new QtWebSocketPacket()
            {
                type = QtWebSocketPacketType.invokeMethod,
                obj = obj,
                method = method,
                args = new JArray(arguments)
            });

            await this.Send(new QtWebSocketPacket()
            {
                type = QtWebSocketPacketType.idle,
            });
        }

        public async Task Send(QtWebSocketPacket packet)
        {
            packet.id = this.PacketID++;

            await base.Send(packet);
        }

        public async Task<QtWebSocketPacket> SendAndReceive(QtWebSocketPacket packet)
        {
            await this.Send(packet);

            QtWebSocketPacket receivedPacket = null;
            await AsyncRunner.WaitForSuccess(() =>
            {
                if (this.receivedPackets.TryGetValue(packet.id, out receivedPacket) && receivedPacket != null)
                {
                    return true;
                }
                return false;
            }, secondsToWait: 5);

            this.receivedPackets.Remove(packet.id);
            return receivedPacket;
        }

        private void QtClientWebSocket_PacketReceived(object sender, string data)
        {
            try
            {
                QtWebSocketPacket packet = JSONSerializerHelper.DeserializeFromString<QtWebSocketPacket>(data);
                if (packet.id >= 0)
                {
                    this.receivedPackets[packet.id] = packet;
                }

                this.QtPacketReceived(this, packet);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
