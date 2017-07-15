using Mixer.Base.ViewModel;
using MixItUp.Base.Overlay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class OverlayAction : ActionBase
    {
        public string FilePath { get; set; }
        public int Duration { get; set; }

        public int Horizontal { get; set; }
        public int Vertical { get; set; }

        public string FileData { get; set; }

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
            if (MixerAPIHandler.OverlayServer != null)
            {
                if (this.FileData == null)
                {
                    byte[] byteData = File.ReadAllBytes(this.FilePath);
                    this.FileData = Convert.ToBase64String(byteData);
                }

                string filePath = string.Concat("file://" + this.FilePath.Replace("\\", "/"));

                MixerAPIHandler.OverlayServer.SetOverlayImage(new OverlayImage()
                {
                    filePath = filePath,
                    fileData = this.FileData,
                    duration = this.Duration,
                    horizontal = this.Horizontal,
                    vertical = this.Vertical
                });
            }
            return Task.FromResult(0);
        }
        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.FilePath, this.Duration.ToString(), this.Horizontal.ToString(), this.Vertical.ToString() }
            };
        }
    }
}
