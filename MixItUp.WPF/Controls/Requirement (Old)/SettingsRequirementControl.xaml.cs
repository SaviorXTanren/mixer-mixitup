using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for SettingsRequirementControl.xaml
    /// </summary>
    public partial class SettingsRequirementControl : UserControl
    {
        public SettingsRequirementControl()
        {
            InitializeComponent();

            this.Loaded += SettingsRequirementControl_Loaded;
        }

        private void SettingsRequirementControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DeleteChatCommandWhenRunToggleSwitch.Visibility == Visibility.Visible)
            {
                if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
                {
                    this.DontDeleteCommandsWhenRunTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    this.DeleteCommandsWhenRunTextBlock.Visibility = Visibility.Visible;
                }
            }

            if (ChannelSession.Services.Patreon.IsConnected && ChannelSession.Services.Patreon.Campaign != null)
            {
                this.EnableDisablePatreonBenefitToggleSwitch.IsEnabled = true;
                this.PatreonBenefitComboBox.ItemsSource = ChannelSession.Services.Patreon.Campaign.Benefits.Values;
            }
        }

        public void HideDeleteChatCommandWhenRun()
        {
            this.DeleteChatCommandWhenRunToggleSwitch.Visibility = Visibility.Collapsed;
        }

        public SettingsRequirementViewModel GetSettingsRequirement()
        {
            if (this.EnableDisablePatreonBenefitToggleSwitch.IsChecked.GetValueOrDefault() && this.PatreonBenefitComboBox.SelectedIndex < 0)
            {
                return null;
            }

            SettingsRequirementViewModel settings = new SettingsRequirementViewModel();
            if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
            {
                settings.DontDeleteChatCommandWhenRun = this.DeleteChatCommandWhenRunToggleSwitch.IsChecked.GetValueOrDefault();
            }
            else
            {
                settings.DeleteChatCommandWhenRun = this.DeleteChatCommandWhenRunToggleSwitch.IsChecked.GetValueOrDefault();
            }

            if (this.EnableDisablePatreonBenefitToggleSwitch.IsChecked.GetValueOrDefault())
            {
                PatreonBenefit benefit = (PatreonBenefit)this.PatreonBenefitComboBox.SelectedItem;
                settings.PatreonBenefitIDRequirement = benefit.ID;
            }

            settings.ShowOnChatMenu = this.ShowOnChatMenuToggleSwitch.IsChecked.GetValueOrDefault();

            return settings;
        }

        public void SetSettingsRequirement(SettingsRequirementViewModel settings)
        {
            if (ChannelSession.Settings.DeleteChatCommandsWhenRun)
            {
                this.DeleteChatCommandWhenRunToggleSwitch.IsChecked = settings.DontDeleteChatCommandWhenRun;
            }
            else
            {
                this.DeleteChatCommandWhenRunToggleSwitch.IsChecked = settings.DeleteChatCommandWhenRun;
            }

            if (!string.IsNullOrEmpty(settings.PatreonBenefitIDRequirement) && ChannelSession.Services.Patreon != null)
            {
                if (ChannelSession.Services.Patreon.Campaign.Benefits.ContainsKey(settings.PatreonBenefitIDRequirement))
                {
                    this.EnableDisablePatreonBenefitToggleSwitch.IsChecked = true;
                    this.PatreonBenefitComboBox.SelectedItem = ChannelSession.Services.Patreon.Campaign.Benefits[settings.PatreonBenefitIDRequirement];
                }
            }

            this.ShowOnChatMenuToggleSwitch.IsChecked = settings.ShowOnChatMenu;
        }

        public async Task<bool> Validate()
        {
            if (this.EnableDisablePatreonBenefitToggleSwitch.IsChecked.GetValueOrDefault())
            {
                if (this.PatreonBenefitComboBox.SelectedIndex < 0)
                {
                    await DialogHelper.ShowMessage("A Patreon Benefit must be specified when Patreon Benefit requirement is set");
                    return false;
                }
            }
            return true;
        }

        private void EnableDisablePatreonBenefitToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.EnableDisablePatreonBenefitToggleSwitch.IsChecked.GetValueOrDefault())
            {
                this.PatreonBenefitComboBox.IsEnabled = true;
            }
            else
            {
                this.PatreonBenefitComboBox.IsEnabled = false;
                this.PatreonBenefitComboBox.SelectedIndex = -1;
            }
        }
    }
}
