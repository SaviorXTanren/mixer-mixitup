using MixItUp.Base;
using MixItUp.Base.Services;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for MixItUpOnlineControl.xaml
    /// </summary>
    public partial class MixItUpOnlineControl : MainControlBase
    {
        public MixItUpOnlineControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            if (ChannelSession.AppSettings.IsDarkBackground)
            {
                this.LightMode.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.DarkMode.Visibility = Visibility.Collapsed;
            }
            await base.InitializeInternal();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ServiceManager.Get<IProcessService>().LaunchLink("https://online.mixitupapp.com/alpha");
        }
    }
}