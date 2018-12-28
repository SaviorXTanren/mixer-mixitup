using MixItUp.Base.ViewModel.Requirement;
using System.Threading.Tasks;
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
        }

        public SettingsRequirementViewModel GetSettingsRequirement()
        {
            SettingsRequirementViewModel settings = new SettingsRequirementViewModel();
            settings.DeleteChatCommandWhenRun = this.DeleteChatCommandWhenRunToggleSwitch.IsChecked.GetValueOrDefault();
            return settings;
        }

        public void SetSettingsRequirement(SettingsRequirementViewModel settings)
        {
            this.DeleteChatCommandWhenRunToggleSwitch.IsChecked = settings.DeleteChatCommandWhenRun;
        }

        public Task<bool> Validate()
        {
            return Task.FromResult(true);
        }
    }
}
