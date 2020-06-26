using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ClipsActionControl.xaml
    /// </summary>
    public partial class ClipsActionControl : ActionControlBase
    {
        private ClipsAction action;

        public ClipsActionControl() : base() { InitializeComponent(); }

        public ClipsActionControl(ClipsAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.IncludeDelayToggleButton.IsChecked = action.IncludeDelay;
                this.ShowClipInfoInChatToggleButton.IsChecked = action.ShowClipInfoInChat;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            return new ClipsAction(this.IncludeDelayToggleButton.IsChecked.GetValueOrDefault(), this.ShowClipInfoInChatToggleButton.IsChecked.GetValueOrDefault());
        }
    }
}