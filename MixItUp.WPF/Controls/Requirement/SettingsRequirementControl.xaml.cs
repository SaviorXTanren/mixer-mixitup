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
            return new SettingsRequirementViewModel();
        }

        public void SetSettingsRequirement(SettingsRequirementViewModel settings)
        {

        }

        public async Task<bool> Validate()
        {
            return true;
        }
    }
}
