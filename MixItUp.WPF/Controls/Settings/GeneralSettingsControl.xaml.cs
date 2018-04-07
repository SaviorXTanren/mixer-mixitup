using MixItUp.Base;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : UserControl
    {
        public GeneralSettingsControl()
        {
            InitializeComponent();

            if (ChannelSession.Settings.DiagnosticLogging)
            {
                //this.DisableDiagnosticLogsButton.Visibility = Visibility.Visible;
            }
            else
            {
                //this.EnableDiagnosticLogsButton.Visibility = Visibility.Visible;
            }

            if (App.AppSettings.DarkTheme)
            {
                //this.SwitchThemeButton.Content = MainMenuControl.SwitchToLightThemeText;
            }
        }
    }
}
