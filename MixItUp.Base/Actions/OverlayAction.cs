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
        public string FilePath { get; set; }

        [DataMember]
        public int Duration { get; set; }

        [DataMember]
        public int Horizontal { get; set; }

        [DataMember]
        public int Vertical { get; set; }

        [DataMember]
        public string FileData { get; set; }

        public OverlayAction() { }

        public OverlayAction(string filePath, int duration, int horizontal, int vertical)
            : base(ActionTypeEnum.Overlay)
        {
            this.FilePath = filePath;
            this.Duration = duration;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.OverlayServer != null)
            {
                if (this.FileData == null)
                {
                    byte[] byteData = File.ReadAllBytes(this.FilePath);
                    this.FileData = Convert.ToBase64String(byteData);
                }

                ChannelSession.OverlayServer.SetOverlayImage(new OverlayImage()
                {
                    filePath = this.FilePath,
                    fileData = this.FileData,
                    duration = this.Duration,
                    horizontal = this.Horizontal,
                    vertical = this.Vertical
                });
            }
            return Task.FromResult(0);
        }
    }
}
