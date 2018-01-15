using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Actions;
using System;

namespace MixItUp.WPF.Controls.Command
{
    public enum BasicCommandTypeEnum
    {
        None,
        Chat,
        Sound,
    }

    public abstract class CommandEditorControlBase : LoadingControlBase
    {
        public event EventHandler<CommandBase> OnCommandSaveSuccessfully;

        public void CommandSavedSuccessfully(CommandBase command)
        {
            if (this.OnCommandSaveSuccessfully != null)
            {
                this.OnCommandSaveSuccessfully(this, command);
            }
        }

        public abstract CommandBase GetExistingCommand();

        public virtual void MoveActionUp(ActionContainerControl actionContainer) { }

        public virtual void MoveActionDown(ActionContainerControl actionContainer) { }

        public virtual void DeleteAction(ActionContainerControl actionContainer) { }
    }
}
