using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for ActionGroupActionControl.xaml
    /// </summary>
    public partial class ActionGroupActionControl : ActionControlBase
    {
        private ActionGroupAction action;

        public ActionGroupActionControl(ActionContainerControl containerControl) : base(containerControl) { InitializeComponent(); }

        public ActionGroupActionControl(ActionContainerControl containerControl, ActionGroupAction action) : this(containerControl) { this.action = action; }

        public override Task OnLoaded()
        {
            this.ActionGroupNameComboBox.ItemsSource = ChannelSession.Settings.ActionGroupCommands.OrderBy(c => c.Name);
            if (this.action != null)
            {
                this.ActionGroupNameComboBox.SelectedItem = this.action.GetCommand();
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (this.ActionGroupNameComboBox.SelectedIndex >= 0)
            {
                ActionGroupCommand command = (ActionGroupCommand)this.ActionGroupNameComboBox.SelectedItem;
                return new ActionGroupAction(command);
            }
            return null;
        }
    }
}
