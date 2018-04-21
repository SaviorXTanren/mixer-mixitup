using MixItUp.Base;
using MixItUp.WPF.Controls.MainControls;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for GeneralSettingsControl.xaml
    /// </summary>
    public partial class GeneralSettingsControl : MainControlBase
    {
        public GeneralSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.FeatureMeToggleButton.IsChecked = ChannelSession.Settings.FeatureMe;

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
    }
}
