using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for UserRoleRequirementControl.xaml
    /// </summary>
    public partial class UserRoleRequirementControl : UserControl
    {
        private RoleRequirementViewModel tempRole = new RoleRequirementViewModel();

        public UserRoleRequirementControl()
        {
            InitializeComponent();

            this.Loaded += RequirementControl_Loaded;
        }

        public RoleRequirementViewModel GetUserRoleRequirement()
        {
            return this.tempRole;
        }

        public void SetUserRoleRequirement(RoleRequirementViewModel role)
        {
            if (role != null)
            {
                this.tempRole = role;
                this.UserRoleComboBox.ItemsSource = RoleRequirementViewModel.BasicUserRoleAllowedValues;
                this.UserRoleComboBox.SelectedItem = role.RoleNameString;
            }
        }

        private void RequirementControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetUserRoleRequirement(this.tempRole);
        }

        private void UserRoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempRole.MixerRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.UserRoleComboBox.SelectedItem);
        }
    }
}
