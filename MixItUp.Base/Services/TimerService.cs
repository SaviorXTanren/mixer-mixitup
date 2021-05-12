using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface ITimerService : IDisposable
    {
        Task Initialize();

        Task RebuildTimerGroups();
    }

    public class TimerService : ITimerService
    {
        private bool isInitialized = false;

        private SemaphoreSlim timerCommandGroupSemaphpre = new SemaphoreSlim(1);
        private Dictionary<string, List<TimerCommandModel>> timerCommandGroups = new Dictionary<string, List<TimerCommandModel>>();
        private Dictionary<string, int> timerCommandIndexes = new Dictionary<string, int>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        private int totalMessages = 0;

        private int groupTotalTime = 0;
        private int nonGroupTotalTime = 0;

        public async Task Initialize()
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;

                GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;

                await this.RebuildTimerGroups();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(this.TimerCommandsBackground, this.backgroundThreadCancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task RebuildTimerGroups()
        {
            await this.timerCommandGroupSemaphpre.WaitAndRelease(() =>
            {
                this.timerCommandGroups.Clear();
                this.timerCommandGroups[string.Empty] = new List<TimerCommandModel>();
                foreach (var kvp in ChannelSession.Settings.CommandGroups)
                {
                    this.timerCommandGroups[kvp.Key] = new List<TimerCommandModel>();
                }

                IEnumerable<TimerCommandModel> timerCommands = ChannelSession.Services.Command.TimerCommands.ToList();
                if (ChannelSession.Settings.RandomizeTimers)
                {
                    timerCommands = timerCommands.Shuffle();
                }

                foreach (TimerCommandModel command in timerCommands)
                {
                    if (command.IsEnabled)
                    {
                        if (string.IsNullOrEmpty(command.GroupName))
                        {
                            this.timerCommandGroups[string.Empty].Add(command);
                        }
                        else if (ChannelSession.Settings.CommandGroups.ContainsKey(command.GroupName))
                        {
                            if (ChannelSession.Settings.CommandGroups[command.GroupName].TimerInterval == 0)
                            {
                                this.timerCommandGroups[string.Empty].Add(command);
                            }
                            else
                            {
                                this.timerCommandGroups[command.GroupName].Add(command);
                            }
                        }
                    }
                }

                this.timerCommandIndexes.Clear();
                foreach (var kvp in this.timerCommandGroups)
                {
                    this.timerCommandIndexes[kvp.Key] = 0;
                }

                return Task.FromResult(0);
            });
        }

        private void GlobalEvents_OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            if (message is UserChatMessageViewModel)
            {
                totalMessages++;
            }
        }

        private async Task TimerCommandsBackground(CancellationToken cancellationToken)
        {
            if (!ChannelSession.Settings.DisableAllTimers)
            {
                List<string> timerGroupsToRun = new List<string>();
                await this.timerCommandGroupSemaphpre.WaitAndRelease(() =>
                {
                    groupTotalTime++;
                    foreach (var kvp in ChannelSession.Settings.CommandGroups)
                    {
                        if (kvp.Value.TimerInterval > 0 && groupTotalTime % kvp.Value.TimerInterval == 0)
                        {
                            if (!timerCommandIndexes.ContainsKey(kvp.Key))
                            {
                                timerCommandIndexes[kvp.Key] = 0;
                            }

                            if (this.timerCommandGroups.ContainsKey(kvp.Key))
                            {
                                timerGroupsToRun.Add(kvp.Key);
                            }
                        }
                    }

                    nonGroupTotalTime++;
                    if (nonGroupTotalTime >= ChannelSession.Settings.TimerCommandsInterval)
                    {
                        if (totalMessages >= ChannelSession.Settings.TimerCommandsMinimumMessages)
                        {
                            timerGroupsToRun.Add(string.Empty);
                            totalMessages = 0;
                            nonGroupTotalTime = 0;
                        }
                    }

                    return Task.FromResult(0);
                });

                foreach (string timerGroupToRun in timerGroupsToRun)
                {
                    await this.RunTimerFromGroup(timerGroupToRun);
                }
            }
        }

        private async Task RunTimerFromGroup(string groupName)
        {
            if (this.timerCommandGroups.ContainsKey(groupName) && this.timerCommandGroups[groupName].Count() > 0)
            {
                if (timerCommandIndexes[groupName] >= this.timerCommandGroups[groupName].Count())
                {
                    timerCommandIndexes[groupName] = 0;
                }

                await ChannelSession.Services.Command.Queue(this.timerCommandGroups[groupName].ElementAt(timerCommandIndexes[groupName]));

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
