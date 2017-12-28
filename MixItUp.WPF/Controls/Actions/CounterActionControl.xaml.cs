using MixItUp.Base.Actions;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for CounterActionControl.xaml
    /// </summary>
    public partial class CounterActionControl : ActionControlBase
    {
        private CounterAction action;

        public CounterActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public CounterActionControl(ActionContainerControl containerControl, CounterAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.CounterNameTextBox.Text = this.action.CounterName;
                this.CounterAmountTextBox.Text = this.action.CounterAmount.ToString();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            int counterAmount;
            if (!string.IsNullOrEmpty(this.CounterNameTextBox.Text) && this.CounterNameTextBox.Text.All(c => char.IsLetterOrDigit(c)) &&
                !string.IsNullOrEmpty(this.CounterAmountTextBox.Text) && int.TryParse(this.CounterAmountTextBox.Text, out counterAmount))
            {
                return new CounterAction(this.CounterNameTextBox.Text, counterAmount);
            }
            return null;
        }
    }
}
