using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ITimerService : IDisposable
    {
        void Initialize();
    }

    public class TimerService : ITimerService
    {
        private bool isInitialized = false;

        private Dictionary<string, int> timerCommandIndexes = new Dictionary<string, int>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        private int totalMessages = 0;

        private int groupTotalTime = 0;
        private int nonGroupTotalTime = 0;

        public void Initialize()
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;

                GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;

                timerCommandIndexes[string.Empty] = 0;

                AsyncRunner.RunBackgroundTask(this.backgroundThreadCancellationTokenSource.Token, 60000, this.TimerCommandsBackground);
            }
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (!(message is AlertChatMessageViewModel))
            {
                totalMessages++;
            }
        }

        private async Task TimerCommandsBackground(CancellationToken cancellationToken)
        {
            if (!ChannelSession.Settings.DisableAllTimers)
            {
                Dictionary<string, List<TimerCommand>> commandGroups = new Dictionary<string, List<TimerCommand>>();
                commandGroups[string.Empty] = new List<TimerCommand>();
                foreach (var kvp in ChannelSession.Settings.CommandGroups)
                {
                    commandGroups[kvp.Key] = new List<TimerCommand>();
                }

                foreach (TimerCommand command in ChannelSession.Settings.TimerCommands)
                {
                    if (string.IsNullOrEmpty(command.GroupName))
                    {
                        commandGroups[string.Empty].Add(command);
                    }
                    else if (ChannelSession.Settings.CommandGroups.ContainsKey(command.GroupName))
                    {
                        if (ChannelSession.Settings.CommandGroups[command.GroupName].TimerInterval == 0)
                        {
                            commandGroups[string.Empty].Add(command);
                        }
                        else
                        {
                            commandGroups[command.GroupName].Add(command);
                        }
                    }
                }

                groupTotalTime++;
                foreach (var kvp in ChannelSession.Settings.CommandGroups)
                {
                    if (kvp.Value.TimerInterval > 0 && groupTotalTime % kvp.Value.TimerInterval == 0)
                    {
                        if (!timerCommandIndexes.ContainsKey(kvp.Key))
                        {
                            timerCommandIndexes[kvp.Key] = 0;
                        }

                        if (commandGroups.ContainsKey(kvp.Key))
                        {
                            await this.RunTimerFromGroup(kvp.Key, commandGroups[kvp.Key]);
                        }
                    }
                }

                nonGroupTotalTime++;
                if (nonGroupTotalTime >= ChannelSession.Settings.TimerCommandsInterval)
                {
                    if (totalMessages >= ChannelSession.Settings.TimerCommandsMinimumMessages)
                    {
                        await this.RunTimerFromGroup(string.Empty, commandGroups[string.Empty]);

                        totalMessages = 0;
                        nonGroupTotalTime = 0;
                    }
                }
            }
        }

        private async Task RunTimerFromGroup(string groupName, IEnumerable<TimerCommand> timers)
        {
            if (timers != null && timers.Count() > 0)
            {
                if (timerCommandIndexes[groupName] >= timers.Count())
                {
                    timerCommandIndexes[groupName] = 0;
                }

                await timers.ElementAt(timerCommandIndexes[groupName]).Perform();

                timerCommandIndexes[groupName]++;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
