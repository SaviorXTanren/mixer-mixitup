using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirements;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class GiveawayMainControlViewModel : WindowControlViewModelBase
    {
        public string Item { get; set; }

        public string TotalTime
        {
            get { return ChannelSession.Settings.GiveawayTimer.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int time) && time > 0)
                {
                    ChannelSession.Settings.GiveawayTimer = time;
                }
                else
                {
                    ChannelSession.Settings.GiveawayTimer = 0;
                }
                this.NotifyPropertyChanged();
            }
        }

        public string ReminderTime
        {
            get { return ChannelSession.Settings.GiveawayReminderInterval.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int time) && time > 0)
                {
                    ChannelSession.Settings.GiveawayReminderInterval = time;
                }
                else
                {
                    ChannelSession.Settings.GiveawayReminderInterval = 0;
                }
                this.NotifyPropertyChanged();
            }
        }

        public string MaxEntries
        {
            get { return ChannelSession.Settings.GiveawayMaximumEntries.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int maxEntries) && maxEntries > 0)
                {
                    ChannelSession.Settings.GiveawayMaximumEntries = maxEntries;
                }
                else
                {
                    ChannelSession.Settings.GiveawayMaximumEntries = 0;
                }
                this.NotifyPropertyChanged();
            }
        }

        public bool RequireClaim
        {
            get { return ChannelSession.Settings.GiveawayRequireClaim; }
            set
            {
                ChannelSession.Settings.GiveawayRequireClaim = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool AllowPastWinners
        {
            get { return ChannelSession.Settings.GiveawayAllowPastWinners; }
            set
            {
                ChannelSession.Settings.GiveawayAllowPastWinners = value;
                this.NotifyPropertyChanged();
            }
        }

        public string Command
        {
            get { return ChannelSession.Settings.GiveawayCommand; }
            set
            {
                ChannelSession.Settings.GiveawayCommand = (!string.IsNullOrEmpty(value)) ? value : string.Empty;
                this.NotifyPropertyChanged();
            }
        }

        public RequirementsSetViewModel Requirements
        {
            get { return requirements; }
            set
            {
                requirements = value;
                this.NotifyPropertyChanged();
            }
        }
        private RequirementsSetViewModel requirements;

        public bool IsRunning { get { return ServiceManager.Get<GiveawayService>().IsRunning; } }
        public bool IsNotRunning { get { return !this.IsRunning; } }

        public string TimeLeft
        {
            get
            {
                string result = (ServiceManager.Get<GiveawayService>().TimeLeft % 60).ToString() + " " + MixItUp.Base.Resources.Seconds;
                if (ServiceManager.Get<GiveawayService>().TimeLeft > 60)
                {
                    result = (ServiceManager.Get<GiveawayService>().TimeLeft / 60).ToString() + " " +  MixItUp.Base.Resources.Minutes +" " + result;
                }
                return result;
            }
        }

        public string WinnerUsername { get { return ServiceManager.Get<GiveawayService>().Winner?.FullDisplayName ?? string.Empty; } }

        public ThreadSafeObservableCollection<GiveawayUser> EnteredUsers { get; private set; } = new ThreadSafeObservableCollection<GiveawayUser>();

        public CommandModelBase GiveawayStartedReminderCommand
        {
            get { return this.giveawayStartedReminderCommand; }
            set
            {
                this.giveawayStartedReminderCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase giveawayStartedReminderCommand;
        public CommandModelBase GiveawayUserJoinedCommand
        {
            get { return this.giveawayUserJoinedCommand; }
            set
            {
                this.giveawayUserJoinedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase giveawayUserJoinedCommand;
        public CommandModelBase GiveawayWinnerSelectedCommand
        {
            get { return this.giveawayWinnerSelectedCommand; }
            set
            {
                this.giveawayWinnerSelectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase giveawayWinnerSelectedCommand;

        public ICommand StartGiveawayCommand { get; set; }
        public ICommand EndGiveawayCommand { get; set; }

        public GiveawayMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GiveawayService.OnGiveawaysChangedOccurred += GiveawayService_OnGiveawaysChangedOccurred;

            this.Requirements = new RequirementsSetViewModel(ChannelSession.Settings.GiveawayRequirementsSet);
            this.Requirements.Currency.RestrictToRequiredAmountOnly = true;

            this.GiveawayStartedReminderCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.GiveawayStartedReminderCommandID);
            this.GiveawayUserJoinedCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.GiveawayUserJoinedCommandID);
            this.GiveawayWinnerSelectedCommand = ChannelSession.Settings.GetCommand(ChannelSession.Settings.GiveawayWinnerSelectedCommandID);

            this.StartGiveawayCommand = this.CreateCommand(async () =>
            {
                IEnumerable<Result> requirementsValidation = await this.Requirements.Validate();
                if (requirementsValidation.Any(r => !r.Success))
                {
                    await DialogHelper.ShowFailedResults(requirementsValidation);
                    return;
                }
                ChannelSession.Settings.GiveawayRequirementsSet = this.Requirements.GetRequirements();

                string result = await ServiceManager.Get<GiveawayService>().Start(this.Item);
                if (!string.IsNullOrEmpty(result))
                {
                    await DialogHelper.ShowMessage(result);
                }
                this.NotifyPropertyChanges();
            });

            this.EndGiveawayCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<GiveawayService>().End();
                this.NotifyPropertyChanges();
            });
        }

        private void GiveawayService_OnGiveawaysChangedOccurred(object sender, bool usersUpdated)
        {
            if (usersUpdated)
            {
                this.EnteredUsers.ClearAndAddRange(ServiceManager.Get<GiveawayService>().Users);
            }
            this.NotifyPropertyChanges();
        }

        private void NotifyPropertyChanges()
        {
            this.NotifyPropertyChanged("EnteredUsers");
            this.NotifyPropertyChanged("EntryAmountRequired");
            this.NotifyPropertyChanged("IsRunning");
            this.NotifyPropertyChanged("IsNotRunning");
            this.NotifyPropertyChanged("TimeLeft");
            this.NotifyPropertyChanged("WinnerUsername");
        }
    }
}
