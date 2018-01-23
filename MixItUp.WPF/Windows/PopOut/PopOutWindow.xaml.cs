using MixItUp.WPF.Controls.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.PopOut
{
    /// <summary>
    /// Interaction logic for PopOutWindow.xaml
    /// </summary>
    public partial class PopOutWindow : LoadingWindowBase
    {
        private MainControlBase mainControl;

        public PopOutWindow(string name, MainControlBase mainControl)
        {
            this.mainControl = mainControl;

            InitializeComponent();

            this.HeaderTextBlock.Text = name;

            this.MainContentControl.Content = this.mainControl;

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            await this.mainControl.Initialize(this);

            await base.OnLoaded();
        }
    }
}
