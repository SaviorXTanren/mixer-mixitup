using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for ThresholdRequirementControl.xaml
    /// </summary>
    public partial class ThresholdRequirementControl : UserControl
    {
        public ThresholdRequirementControl()
        {
            InitializeComponent();

            this.AmountTextBox.Text = "0";
            this.TimeSpanTextBox.Text = "0";
        }

        public int GetAmount()
        {
            int amount = 0;
            if (!string.IsNullOrEmpty(this.AmountTextBox.Text))
            {
                int.TryParse(this.AmountTextBox.Text, out amount);
            }
            return amount;
        }

        public int GetTimeSpan()
        {
            int amount = 0;
            if (!string.IsNullOrEmpty(this.TimeSpanTextBox.Text))
            {
                int.TryParse(this.TimeSpanTextBox.Text, out amount);
            }
            return amount;
        }

        public ThresholdRequirementViewModel GetThresholdRequirement()
        {
            if (this.GetAmount() >= 0 && this.GetTimeSpan() >= 0)
            {
                return new ThresholdRequirementViewModel(this.GetAmount(), this.GetTimeSpan());
            }
            return new ThresholdRequirementViewModel();
        }

        public void SetThresholdRequirement(ThresholdRequirementViewModel threshold)
        {
            if (threshold != null)
            {
                this.AmountTextBox.Text = threshold.Amount.ToString();
                this.TimeSpanTextBox.Text = threshold.TimeSpan.ToString();
            }
        }

        public async Task<bool> Validate()
        {
            if (this.GetAmount() < 0)
            {
                await DialogHelper.ShowMessage("The Threshold required user amount must be greater than or equal to 0");
                return false;
            }

            if (this.GetTimeSpan() < 0)
            {
                await DialogHelper.ShowMessage("The Threshold time span must be greater than or equal to 0");
                return false;
            }

            return true;
        }
    }
}
