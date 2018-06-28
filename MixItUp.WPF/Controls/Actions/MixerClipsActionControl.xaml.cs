using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for MixerClipsActionControl.xaml
    /// </summary>
    public partial class MixerClipsActionControl : ActionControlBase
    {
        private MixerClipsAction action;

        public MixerClipsActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public MixerClipsActionControl(ActionContainerControl containerControl, MixerClipsAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.ClipLengthTextBox.Text = "30";
            if (this.action != null)
            {
                this.ClipNameTextBox.Text = this.action.ClipName;
                this.ClipLengthTextBox.Text = this.action.ClipLength.ToString();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.ClipNameTextBox.Text) && !string.IsNullOrEmpty(this.ClipLengthTextBox.Text))
            {
                if (int.TryParse(this.ClipLengthTextBox.Text, out int length) && MixerClipsAction.MinimumLength <= length && length <= MixerClipsAction.MaximumLength)
                {
                    return new MixerClipsAction(this.ClipNameTextBox.Text, length);
                }
            }
            return null;
        }
    }
}
