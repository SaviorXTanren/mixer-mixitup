using MixItUp.Base;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : SettingsControlBase
    {
        public static readonly string AutoLogInTooltip =
            "The Auto Log-In setting allows Mix It Up to automatically" + Environment.NewLine +
            "log in the currently signed-in Mixer user account." + Environment.NewLine + Environment.NewLine +
            "NOTE: If there is a Mix It Up update or you are required" + Environment.NewLine +
            "to re-authenticate any of your services accounts, you will" + Environment.NewLine +
            "need to manually log in when this occurs.";

        public GeneralSettingsControl()
        {
            InitializeComponent();

            this.AutoLogInAccountTextBlock.ToolTip = AutoLogInTooltip;
            this.AutoLogInAccountToggleButton.ToolTip = AutoLogInTooltip;
        }

        protected override async Task InitializeInternal()
        {
            this.FeatureMeToggleButton.IsChecked = ChannelSession.Settings.FeatureMe;
            this.AutoLogInAccountToggleButton.IsChecked = (App.AppSettings.AutoLogInAccount == ChannelSession.User.id);

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void FeatureMeToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ChannelSession.Settings.FeatureMe = this.FeatureMeToggleButton.IsChecked.GetValueOrDefault();
        }

        private void AutoLogInAccountToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.AppSettings.AutoLogInAccount = ChannelSession.User.id;
        }

        private void AutoLogInAccountToggleButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.AppSettings.AutoLogInAccount = 0;
        }
    }
}
