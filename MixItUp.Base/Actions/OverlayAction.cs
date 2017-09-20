using MixItUp.Base.Overlay;
using MixItUp.Base.ViewModel;
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
        [DataMember]
        public string ImagePath;

        [DataMember]
        public string Text;
        [DataMember]
        public string Color;

        [DataMember]
        public int Duration;
        [DataMember]
        public int Horizontal;
        [DataMember]
        public int Vertical;

        private string imageData { get; set; }

        public OverlayAction() { }

        public OverlayAction(string imagePath, int duration, int horizontal, int vertical)
            : base(ActionTypeEnum.Overlay)
        {
            this.ImagePath = imagePath;
        }

        public OverlayAction(string text, string color, int duration, int horizontal, int vertical)
            : this(duration, horizontal, vertical)
        {
            this.Text = text;
            this.Color = color;
        }

        public OverlayAction(int duration, int horizontal, int vertical)
            : base(ActionTypeEnum.Overlay)
        {
            this.Duration = duration;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.OverlayServer != null)
            {
                if (this.ImagePath != null)
                {
                    if (this.imageData == null)
                    {
                        byte[] byteData = File.ReadAllBytes(this.ImagePath);
                        this.imageData = Convert.ToBase64String(byteData);
                    }
                    ChannelSession.OverlayServer.SetImage(new OverlayImage()
                    {
                        imagePath = this.ImagePath, duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical, imageData = this.imageData
                    });
                }
                else if (this.Text != null)
                {
                    string text = this.ReplaceStringWithSpecialModifiers(this.Text, user, arguments);
                    ChannelSession.OverlayServer.SetText(new OverlayText()
                    {
                        text = text, color = this.Color, duration = this.Duration, horizontal = this.Horizontal, vertical = this.Vertical
                    });
                }
            }
            return Task.FromResult(0);
        }
    }
}
