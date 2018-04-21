using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for TwitterActionControl.xaml
    /// </summary>
    public partial class TwitterActionControl : ActionControlBase
    {
        private TwitterAction action;

        public TwitterActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public TwitterActionControl(ActionContainerControl containerControl, TwitterAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.TweetMessageTextBox.Text = this.action.TweetText;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.TweetMessageTextBox.Text))
            {
                return new TwitterAction(this.TweetMessageTextBox.Text);
            }
            return null;
        }
    }
}
