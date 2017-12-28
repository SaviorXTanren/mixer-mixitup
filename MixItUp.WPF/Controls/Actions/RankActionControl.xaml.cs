using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for RankActionControl.xaml
    /// </summary>
    public partial class RankActionControl : ActionControlBase
    {
        private RankAction action;

        public RankActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public RankActionControl(ActionContainerControl containerControl, RankAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.RankAmountTextBox.Text = this.action.Amount.ToString();
                this.RankMessageTextBox.Text = this.action.ChatText;
                this.RankWhisperToggleButton.IsChecked = this.action.IsWhisper;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            int rankAmount;
            if (!string.IsNullOrEmpty(this.RankAmountTextBox.Text) && int.TryParse(this.RankAmountTextBox.Text, out rankAmount))
            {
                return new RankAction(rankAmount, this.RankMessageTextBox.Text, this.RankWhisperToggleButton.IsChecked.GetValueOrDefault());
            }
            return null;
        }
    }
}
