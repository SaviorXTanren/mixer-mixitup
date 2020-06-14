using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for RoleRequirementControl.xaml
    /// </summary>
    public partial class RoleRequirementControl : UserControl
    {
        private RoleRequirementViewModel tempRole = new RoleRequirementViewModel();

        public RoleRequirementControl()
        {
            InitializeComponent();

            this.Loaded += RequirementControl_Loaded;
        }

        public RoleRequirementViewModel GetRoleRequirement()
        {
            return this.tempRole;
        }

        public void SetRoleRequirement(RoleRequirementViewModel role)
        {
            if (role != null)
            {
                this.tempRole = role;
                this.RoleComboBox.ItemsSource = MixItUp.Base.ViewModel.Requirements.RoleRequirementViewModel.SelectableUserRoles();
                this.RoleComboBox.SelectedItem = role.MixerRole;
            }
        }

        private void RequirementControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetRoleRequirement(this.tempRole);
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempRole.MixerRole = (UserRoleEnum)this.RoleComboBox.SelectedItem;
        }
    }
}
