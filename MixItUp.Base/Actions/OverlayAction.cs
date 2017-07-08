using Mixer.Base.ViewModel;
using MixItUp.Base.Overlay;
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

        public OverlayAction()
            : base("Overlay")
        {
            this.Width = -1;
            this.Height = -1;
        }

        public override Task Perform(UserViewModel user)
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
    }
}
