using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for CooldownRequirementControl.xaml
    /// </summary>
    public partial class CooldownRequirementControl : UserControl
    {
        public CooldownRequirementControl()
        {
            InitializeComponent();

            this.CooldownTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CooldownTypeEnum>().OrderBy(c => c);
            this.CooldownTypeComboBox.SelectedItem = EnumHelper.GetEnumName(CooldownTypeEnum.Static);
            this.CooldownAmountTextBox.Text = "0";

            IEnumerable<PermissionsCommandBase> permissionCommands = ChannelSession.AllCommands.Where(c => c is PermissionsCommandBase).Select(c => (PermissionsCommandBase)c);
            permissionCommands = permissionCommands.Where(c => c.Requirements.Cooldown != null && c.Requirements.Cooldown.IsGroup);
            this.CooldownGroupsComboBox.ItemsSource = permissionCommands.Select(c => c.Requirements.Cooldown.GroupName).Distinct();
        }

        public int GetCooldownAmount()
        {
            int amount = -1;
            if (!string.IsNullOrEmpty(this.CooldownAmountTextBox.Text))
            {
                int.TryParse(this.CooldownAmountTextBox.Text, out amount);
            }
            return amount;
        }

        public CooldownRequirementViewModel GetCooldownRequirement()
        {
            if (this.CooldownTypeComboBox.SelectedIndex >= 0 && this.GetCooldownAmount() >= 0)
            {
                CooldownTypeEnum type = EnumHelper.GetEnumValueFromString<CooldownTypeEnum>((string)this.CooldownTypeComboBox.SelectedItem);
                if (type == CooldownTypeEnum.Group)
                {
                    if (string.IsNullOrEmpty(this.CooldownGroupsComboBox.Text))
                    {
                        return null;
                    }
                    return new CooldownRequirementViewModel(type, this.CooldownGroupsComboBox.Text, this.GetCooldownAmount());
                }
                else
                {
                    return new CooldownRequirementViewModel(type, this.GetCooldownAmount());
                }
            }
            return new CooldownRequirementViewModel();
        }

        public void SetCooldownRequirement(CooldownRequirementViewModel cooldown)
        {
            if (cooldown != null)
            {
                this.CooldownTypeComboBox.SelectedItem = EnumHelper.GetEnumName(cooldown.Type);
                if (cooldown.IsGroup && ChannelSession.Settings.CooldownGroups.ContainsKey(cooldown.GroupName))
                {
                    this.CooldownGroupsComboBox.Text = cooldown.GroupName;
                    this.CooldownAmountTextBox.Text = ChannelSession.Settings.CooldownGroups[cooldown.GroupName].ToString();
                }
                else
                {
                    this.CooldownAmountTextBox.Text = cooldown.Amount.ToString();
                }
            }
        }

        public async Task<bool> Validate()
        {
            if (this.GetCooldownAmount() < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                return false;
            }

            CooldownTypeEnum type = EnumHelper.GetEnumValueFromString<CooldownTypeEnum>((string)this.CooldownTypeComboBox.SelectedItem);
            if (type == CooldownTypeEnum.Group && string.IsNullOrEmpty(this.CooldownGroupsComboBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A Cooldown Group must be specified");
                return false;
            }

            return true;
        }

        private void CooldownTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CooldownTypeComboBox.SelectedIndex >= 0)
            {
                CooldownTypeEnum type = EnumHelper.GetEnumValueFromString<CooldownTypeEnum>((string)this.CooldownTypeComboBox.SelectedItem);
                this.CooldownGroupsComboBox.Visibility = (type == CooldownTypeEnum.Group) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CooldownGroupsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CooldownGroupsComboBox.SelectedIndex >= 0)
            {
                string cooldownGroup = (string)this.CooldownGroupsComboBox.SelectedItem;
                if (ChannelSession.Settings.CooldownGroups.ContainsKey(cooldownGroup))
                {
                    this.CooldownAmountTextBox.Text = ChannelSession.Settings.CooldownGroups[cooldownGroup].ToString();
                }
                else
                {
                    this.CooldownAmountTextBox.Text = "0";
                }
            }
        }
    }
}
