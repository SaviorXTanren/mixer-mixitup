using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for SongRequestActionControl.xaml
    /// </summary>
    public partial class SongRequestActionControl : ActionControlBase
    {
        private SongRequestAction action;

        public SongRequestActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public SongRequestActionControl(ActionContainerControl containerControl, SongRequestAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.SongRequestActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<SongRequestActionTypeEnum>().OrderBy(s => s);
            if (this.action != null)
            {
                this.SongRequestActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(action.SongRequestType);
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.SongRequestActionTypeComboBox.SelectedIndex >= 0)
            {
                SongRequestActionTypeEnum actionType = EnumHelper.GetEnumValueFromString<SongRequestActionTypeEnum>((string)this.SongRequestActionTypeComboBox.SelectedItem);
                return new SongRequestAction(actionType);
            }
            return null;
        }
    }
}
