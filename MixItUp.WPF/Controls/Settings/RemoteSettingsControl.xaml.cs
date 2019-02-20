using MixItUp.Base.ViewModel.Controls.Settings;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for RemoteSettingsControl.xaml
    /// </summary>
    public partial class RemoteSettingsControl : SettingsControlBase
    {
        public RemoteSettingsControl()
        {
            InitializeComponent();

            this.DataContext = new RemoteSettingsControlViewModel();
        }
    }
}
