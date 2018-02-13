using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for UserRoleRequirementControl.xaml
    /// </summary>
    public partial class UserRoleRequirementControl : UserControl
    {
        private UserRole tempRole = UserRole.User;

        public UserRoleRequirementControl()
        {
            InitializeComponent();

            this.Loaded += RequirementControl_Loaded;
        }

        public UserRole GetUserRoleRequirement()
        {
            return this.tempRole;
        }

        public void SetUserRoleRequirement(UserRole role)
        {
            if (role != UserRole.Banned)
            {
                this.tempRole = role;
                this.UserRoleComboBox.ItemsSource = RequirementViewModel.UserRoleAllowedValues;
                this.UserRoleComboBox.SelectedItem = EnumHelper.GetEnumName(role);
            }
        }

        private void RequirementControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetUserRoleRequirement(this.tempRole);
        }

        private void UserRoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tempRole = EnumHelper.GetEnumValueFromString<UserRole>((string)this.UserRoleComboBox.SelectedItem);
        }
    }
}
