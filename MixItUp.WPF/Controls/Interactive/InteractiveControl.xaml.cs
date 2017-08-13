using MixItUp.Base;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Interactive
{
    /// <summary>
    /// Interaction logic for InteractiveControl.xaml
    /// </summary>
    public partial class InteractiveControl : MainControlBase
    {
        public InteractiveControl()
        {
            InitializeComponent();
        }

        protected override Task InitializeInternal()
        {
            MixerAPIHandler.InitializeOverlayServer();
            return Task.FromResult(0);
        }
    }
}
