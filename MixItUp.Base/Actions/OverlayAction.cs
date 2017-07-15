using Mixer.Base.ViewModel;
using MixItUp.Base.Overlay;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class OverlayAction : ActionBase
    {
        public string FilePath { get; set; }
        public int Duration { get; set; }

        public int Horizontal { get; set; }
        public int Vertical { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public OverlayAction(string filePath, int duration, int horizontal, int vertical, int width = -1, int height = -1)
            : base(ActionTypeEnum.Overlay)
        {
            this.FilePath = filePath;
            this.Duration = duration;
            this.Horizontal = horizontal;
            this.Vertical = vertical;
            this.Width = width;
            this.Height = height;
        }

        public override Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (MixerAPIHandler.OverlayServer != null)
            {
                MixerAPIHandler.OverlayServer.SetOverlayImage(new OverlayImage()
                {
                    filePath = this.FilePath,
                    duration = this.Duration,
                    horizontal = this.Horizontal,
                    vertical = this.Vertical,
                    width = this.Width,
                    height = this.Height
                });
            }
            return Task.FromResult(0);
        }
        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.FilePath, this.Duration.ToString(), this.Horizontal.ToString(), this.Vertical.ToString(),
                    this.Width.ToString(), this.Height.ToString() }
            };
        }
    }
}
