using Mixer.Base.Util;
using MixItUp.Base;
using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SpotifyActionControl.xaml
    /// </summary>
    public partial class SpotifyActionControl : ActionControlBase
    {
        private SpotifyAction action;

        public SpotifyActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public SpotifyActionControl(ActionContainerControl containerControl, SpotifyAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            if (ChannelSession.Services.Spotify == null)
            {
                this.SpotifyNotEnabledWarningTextBlock.Visibility = System.Windows.Visibility.Visible;
            }

            this.SpotifyActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<SpotifyActionTypeEnum>();
            if (this.action != null)
            {
                this.SpotifyActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(action.SpotifyType);
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.SpotifyActionTypeComboBox.SelectedIndex >= 0)
            {
                SpotifyActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<SpotifyActionTypeEnum>((string)this.SpotifyActionTypeComboBox.SelectedItem);
                return new SpotifyAction(actionType);
            }
            return null;
        }
    }
}
