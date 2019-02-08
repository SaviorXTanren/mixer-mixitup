using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Services
{
    public partial class StreamDeckServiceControl : ServicesControlBase
    {
        public StreamDeckServiceControl()
        {
            InitializeComponent();
        }

        protected override async Task OnLoaded()
        {
            this.SetHeaderText("Stream Deck");
            await base.OnLoaded();
        }
    }
}
