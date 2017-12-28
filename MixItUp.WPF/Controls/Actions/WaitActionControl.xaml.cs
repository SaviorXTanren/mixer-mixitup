using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for WaitActionControl.xaml
    /// </summary>
    public partial class WaitActionControl : ActionControlBase
    {
        private WaitAction action;

        public WaitActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public WaitActionControl(ActionContainerControl containerControl, WaitAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.WaitAmountTextBox.Text = this.action.WaitAmount.ToString();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            double waitAmount;
            if (!string.IsNullOrEmpty(this.WaitAmountTextBox.Text) && double.TryParse(this.WaitAmountTextBox.Text, out waitAmount) && waitAmount > 0.0)
            {
                return new WaitAction(waitAmount);
            }
            return null;
        }
    }
}
