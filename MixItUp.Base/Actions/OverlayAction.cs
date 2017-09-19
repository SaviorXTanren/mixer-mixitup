using MixItUp.Base.Overlay;
using MixItUp.Base.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class OverlayAction : ActionBase
    {
        [JsonProperty]
        public OverlayImage Image { get; set; }

        [JsonProperty]
        public OverlayText Text { get; set; }

        public OverlayAction() { }

        public OverlayAction(OverlayImage image)
            : base(ActionTypeEnum.Overlay)
        {
            this.Image = image;
        }

        public OverlayAction(OverlayText text)
            : base(ActionTypeEnum.Overlay)
        {
            this.Text = text;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.OverlayServer != null)
            {
                if (this.Image != null)
                {
                    if (this.Image.imageData == null)
                    {
                        byte[] byteData = File.ReadAllBytes(this.Image.imagePath);
                        this.Image.imageData = Convert.ToBase64String(byteData);
                    }

                    ChannelSession.OverlayServer.SetImage(this.Image);
                }
                else if (this.Text != null)
                {
                    ChannelSession.OverlayServer.SetText(this.Text);
                }
            }
            return Task.FromResult(0);
        }
    }
}
