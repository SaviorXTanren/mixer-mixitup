using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TimerMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public string TimeIntervalString
        {
            get { return ChannelSession.Settings.TimerCommandsInterval.ToString(); }
            set
            {
                ChannelSession.Settings.TimerCommandsInterval = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
                this.CheckIfMinMessagesAndIntervalAreBothZero();
            }
        }

        public string MinimumMessagesString
        {
            get { return ChannelSession.Settings.TimerCommandsMinimumMessages.ToString(); }
            set
            {
                ChannelSession.Settings.TimerCommandsMinimumMessages = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
                this.CheckIfMinMessagesAndIntervalAreBothZero();
            }
        }

        public bool RunTimersOnlyWhenLive
        {
            get { return ChannelSession.Settings.RunTimersOnlyWhenLive; }
            set
            {
                ChannelSession.Settings.RunTimersOnlyWhenLive = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool RandomizeTimers
        {
            get { return ChannelSession.Settings.RandomizeTimers; }
            set
            {
                ChannelSession.Settings.RandomizeTimers = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool DisableAllTimers
        {
            get { return ChannelSession.Settings.DisableAllTimers; }
            set
            {
                ChannelSession.Settings.DisableAllTimers = value;
                this.NotifyPropertyChanged();

                ServiceManager.Get<TimerService>().RebuildTimerGroups().Wait();
            }
        }

        public TimerMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ServiceManager.Get<CommandService>().TimerCommands.ToList();
        }

        private void CheckIfMinMessagesAndIntervalAreBothZero()
        {
            if (ChannelSession.Settings.TimerCommandsMinimumMessages <= 0 && ChannelSession.Settings.TimerCommandsInterval <= 0)
            {
                this.TimeIntervalString = "1";
            }
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.Timer)
            {
                this.AddCommand(command);
            }
        }
    }
}
