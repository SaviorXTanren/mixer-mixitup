using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
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

            this.CooldownTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<CooldownTypeEnum>();
            this.CooldownTypeComboBox.SelectedItem = EnumHelper.GetEnumName(CooldownTypeEnum.Global);
            this.CooldownAmountTextBox.Text = "0";
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
                return new CooldownRequirementViewModel(EnumHelper.GetEnumValueFromString<CooldownTypeEnum>((string)this.CooldownTypeComboBox.SelectedItem), this.GetCooldownAmount());
            }
            return new CooldownRequirementViewModel();
        }

        public void SetCooldownRequirement(CooldownRequirementViewModel cooldown)
        {
            if (cooldown != null)
            {
                this.CooldownTypeComboBox.SelectedItem = EnumHelper.GetEnumName(cooldown.Type);
                this.CooldownAmountTextBox.Text = cooldown.Amount.ToString();
            }
        }

        public async Task<bool> Validate()
        {
            if (this.GetCooldownAmount() < 0)
            {
                await MessageBoxHelper.ShowMessageDialog("Cooldown must be 0 or greater");
                return false;
            }
            return true;
        }
    }
}
