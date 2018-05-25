using Mixer.Base.Util;
using MixItUp.Base.Actions;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for GameQueueActionControl.xaml
    /// </summary>
    public partial class GameQueueActionControl : ActionControlBase
    {
        private GameQueueAction action;

        public GameQueueActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public GameQueueActionControl(ActionContainerControl containerControl, GameQueueAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.GameQueueActionTypeComboBox.ItemsSource = EnumHelper.GetEnumNames<GameQueueActionType>().OrderBy(s => s);
            if (this.action != null)
            {
                this.GameQueueActionTypeComboBox.SelectedItem = EnumHelper.GetEnumName(this.action.GameQueueType);
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.GameQueueActionTypeComboBox.SelectedIndex >= 0)
            {
                GameQueueActionType gameQueueType = EnumHelper.GetEnumValueFromString<GameQueueActionType>((string)this.GameQueueActionTypeComboBox.SelectedItem);
                return new GameQueueAction(gameQueueType);
            }
            return null;
        }
    }
}
