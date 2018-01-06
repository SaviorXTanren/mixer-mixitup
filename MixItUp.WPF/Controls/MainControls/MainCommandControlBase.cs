using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.WPF.Controls.Command;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    public class MainCommandControlBase : MainControlBase
    {
        public async Task HandleCommandPlay(object sender)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null && commandButtonsControl.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)commandButtonsControl.DataContext;
                await command.PerformAndWait(ChannelSession.GetCurrentUser(), new List<string>() { "@" + ChannelSession.GetCurrentUser().UserName });
                commandButtonsControl.SwitchToPlay();
            }
        }

        public void HandleCommandStop(object sender)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null && commandButtonsControl.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)commandButtonsControl.DataContext;
                command.StopCurrentRun();
                commandButtonsControl.SwitchToPlay();
            }
        }

        public void HandleCommandEnableDisable(object sender)
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null && commandButtonsControl.DataContext is CommandBase)
            {
                CommandBase command = (CommandBase)commandButtonsControl.DataContext;
                command.IsEnabled = commandButtonsControl.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
            }
        }

        public T GetCommandFromCommandButtons<T>(object sender) where T : CommandBase
        {
            CommandButtonsControl commandButtonsControl = (CommandButtonsControl)sender;
            if (commandButtonsControl.DataContext != null && commandButtonsControl.DataContext is CommandBase)
            {
                return (T)commandButtonsControl.DataContext;
            }
            return null;
        }
    }
}
