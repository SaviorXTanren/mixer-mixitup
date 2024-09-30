using MixItUp.Base.Services;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayJavascriptScriptV3Model : OverlayItemV3ModelBase
    {
        public const string ScriptCompletePacketType = "ScriptComplete";

        private bool completed;
        private string result;

#pragma warning disable CS0612 // Type or member is obsolete
        public OverlayJavascriptScriptV3Model(string javascript) : base(OverlayItemV3Type.JavascriptScript) { this.Javascript = javascript; }
#pragma warning restore CS0612 // Type or member is obsolete

        public override async Task ProcessPacket(OverlayV3Packet packet)
        {
            await base.ProcessPacket(packet);

            if (string.Equals(packet.Type, OverlayJavascriptScriptV3Model.ScriptCompletePacketType))
            {
                if (packet.Data.TryGetValue("Result", out JToken value) && value != null)
                {
                    this.result = value.ToString();
                }
                this.completed = true;
            }
        }

        public async Task<string> WaitForResult()
        {
            while (!this.completed)
            {
                await Task.Delay(1000);
            }
            return this.result;
        }
    }
}
