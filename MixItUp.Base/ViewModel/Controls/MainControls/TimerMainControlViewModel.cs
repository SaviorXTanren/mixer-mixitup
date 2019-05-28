using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Controls.MainControls
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

        public bool DisableAllTimers
        {
            get { return ChannelSession.Settings.DisableAllTimers; }
            set
            {
                ChannelSession.Settings.DisableAllTimers = value;
                this.NotifyPropertyChanged();
            }
        }

        public TimerMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override IEnumerable<CommandBase> GetCommands()
        {
            return ChannelSession.Settings.TimerCommands.ToList();
        }

        private void CheckIfMinMessagesAndIntervalAreBothZero()
        {
            if (ChannelSession.Settings.TimerCommandsMinimumMessages <= 0 && ChannelSession.Settings.TimerCommandsInterval <= 0)
            {
                this.TimeIntervalString = "1";
            }
        }
    }
}
