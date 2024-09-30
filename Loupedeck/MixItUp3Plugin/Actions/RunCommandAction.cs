using Loupedeck;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Loupedeck.MixItUp3Plugin.Actions
{
    public class RunCommandAction : PluginDynamicCommand
    {
        private CancellationTokenSource cancellationTokenSource;

        protected override Boolean OnLoad()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            _ = this.BackgroundRefreshAsync(this.cancellationTokenSource.Token);
            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();
            return base.OnUnload();
        }

        //protected override bool OnLoad()
        //{
        //    //this.cancellationTokenSource = new CancellationTokenSource();
        //    //_ = BackgroundRefreshAsync(this.cancellationTokenSource.Token);
        //    return true;
        //}

        //protected override bool OnUnload()
        //{
        //    //this.cancellationTokenSource.Cancel();
        //    //this.cancellationTokenSource.Dispose();
        //    return true;
        //}

        // This method is called when the user executes the command.
        protected override async void RunCommand(String actionParameter)
        {
            if (Guid.TryParse(actionParameter, out var commandId))
            {
                await MixItUp.API.Commands.RunCommandAsync(commandId);
            }
        }

        private async Task BackgroundRefreshAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var allCommands = await MixItUp.API.Commands.GetAllCommandsAsync();

                    // Remove deleted commands
                    var allParameters = this.GetParameters();
                    var toRemove = allParameters.Where(p => !allCommands.Any(c => c.ID.ToString() == p.Name));
                    foreach (var remove in toRemove)
                    {
                        this.RemoveParameter(remove.Name);
                    }

                    // Add new commands
                    foreach (var command in allCommands)
                    {
                        if (this.TryGetParameter(command.ID.ToString(), out var parameter))
                        {
                            parameter.DisplayName = command.Name;
                            parameter.GroupName = command.Category;
                            parameter.SuperGroupName = command.GroupName;
                        }
                        else
                        {
                            this.AddParameter(command.ID.ToString(), command.Name, command.Category, command.GroupName);
                        }
                    }

                    this.ParametersChanged();
                    this.Plugin.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, string.Empty);
                }
                catch
                {
                    this.ParametersChanged();
                    this.Plugin.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Mix It Up is not running or developer APIs are not enabled.", "https://wiki.mixitupapp.com/en/services/loupedeck", "Loupedeck Plug-In Details");
                }

                await Task.Delay(5000);
            }
        }
    }
}
