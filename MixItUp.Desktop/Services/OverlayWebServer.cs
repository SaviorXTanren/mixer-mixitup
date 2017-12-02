using Mixer.Base.Model.Client;
using MixItUp.Base.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MixItUp.Overlay
{
    public class OverlayPacket : WebSocketPacket
    {
        public JObject data;

        public OverlayPacket() { }

        public OverlayPacket(string type, JObject data)
        {
            this.type = type;
            this.data = data;
        }
    }

    public class OverlayWebServer : WebSocketServerBase, IOverlayService
    {
        private OverlayRoluetteWheel lastRoluetteSpin = null;

        public OverlayWebServer(string address) : base(address) { }

        public async Task TestConnection() { await this.Send(new OverlayPacket("test", new JObject())); }

        public async Task SetImage(OverlayImage image) { await this.Send(new OverlayPacket("image", JObject.FromObject(image))); }

        public async Task SetText(OverlayText text) { await this.Send(new OverlayPacket("text", JObject.FromObject(text))); }

        public async Task SetHTMLText(OverlayHTML htmlText) { await this.Send(new OverlayPacket("htmlText", JObject.FromObject(htmlText))); }

        public async Task SetRouletteWheel(OverlayRoluetteWheel roluetteWheel)
        {
            this.lastRoluetteSpin = roluetteWheel;
            await this.Send(new OverlayPacket("rouletteWheel", JObject.FromObject(roluetteWheel)));
        }

        protected override async Task PacketReceived(string packet)
        {
            if (packet != null)
            {
                JObject packetObj = JObject.Parse(packet);
                if (packetObj["type"].ToString().Equals("rouletteWheelResult") && this.lastRoluetteSpin != null)
                {
                    string winningSegment = packetObj["data"].ToString();
                    await RoluetteSpinChatCommand.MessageRoluetteSpinResults(winningSegment, this.lastRoluetteSpin.userID, this.lastRoluetteSpin.bet);
                }
            }
        }
    }
}
