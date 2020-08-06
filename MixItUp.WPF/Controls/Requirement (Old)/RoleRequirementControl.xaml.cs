using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.Requirement;
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

                if (role.SubscriberTier > 0)
                {
                    this.SubTierComboBox.SelectedIndex = role.SubscriberTier - 1;
                }
                else
                {
                    this.SubTierComboBox.SelectedIndex = 0;
                }
            }
        }

        private void RequirementControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SubTierComboBox.ItemsSource = RoleRequirementViewModel.SubTierNames;

            this.SetRoleRequirement(this.tempRole);
        }

        private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempRole.MixerRole = (UserRoleEnum)this.RoleComboBox.SelectedItem;
            this.SubTierComboBox.Visibility = (this.tempRole.MixerRole == UserRoleEnum.Subscriber) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SubTierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempRole.SubscriberTier = this.SubTierComboBox.SelectedIndex + 1;
        }
    }
}
